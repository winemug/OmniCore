using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Eros
{
    public class ErosPodManager : IPodManager
    {
        //private async Task send_request(IRequest request, bool with_nonce = false)
        //{
        //if (with_nonce)
        //{
        //    var nonce_val = this.Nonce.GetNext();
        //    request.set_nonce(nonce_val);
        //    nonce_syncword = null;
        //}

        //var me = new MessageExchange(request, this.packetRadio, this.Pod);

        //var response = await me.GetPodResponse();
        //ProtocolHelper.response_parse(response, this.Pod);

        //if (with_nonce && nonce_syncword != null)
        //{
        //    Debug.WriteLine("Nonce resync requested");
        //    this.Nonce.Sync(request.sequence.Value);
        //    var nonce_val = this.Nonce.GetNext();
        //    request.set_nonce(nonce_val);
        //    nonce_syncword = null;
        //    radio_message_sequence = request.sequence.Value;
        //    response = await me.GetPodResponse();
        //    ProtocolHelper.response_parse(response, this.Pod);
        //    if (nonce_syncword != null)
        //    {
        //        this.Nonce.Reset();
        //        throw new PdmException("Nonce sync failed");
        //    }
        //}
        //}

        public ErosPod ErosPod { get; private set; }
        public IPod Pod { get => ErosPod; }

        readonly IMessageExchangeProvider MessageExchangeProvider;
        readonly Task MessageExchangeTask;
        readonly Nonce Nonce;

        public ErosPodManager(ErosPod pod, IMessageExchangeProvider messageExchangeProvider)
        {
            Nonce = new Nonce(pod);
            ErosPod = pod;
            MessageExchangeProvider = messageExchangeProvider;
            MessageExchangeTask = Task.Run(() => { });
        }

        private ErosMessageExchangeParameters GetStandardParameters()
        {
            return new ErosMessageExchangeParameters() { Nonce = Nonce };
        }

        private async Task<IMessageExchangeResult> PerformExchange(IMessage requestMessage, IMessageExchangeParameters messageExchangeParameters,
                    IMessageExchangeProgress messageProgress, CancellationToken ct)
        {

            var emp = messageExchangeParameters as ErosMessageExchangeParameters;

            return await await MessageExchangeTask.ContinueWith(
                    async (IMessageExchangeResult) =>
                    {
                        try
                        {
                            ErosPod.RuntimeVariables.NonceSync = null;
                            var messageExchange = await MessageExchangeProvider.GetMessageExchanger(messageExchangeParameters, Pod);
                            await messageExchange.InitializeExchange(messageProgress, ct);
                            var response = await messageExchange.GetResponse(requestMessage, messageProgress, ct);
                            var result = messageExchange.ParseResponse(response, Pod);

                            if (result.Success && ErosPod.RuntimeVariables.NonceSync.HasValue)
                            {
                                var responseMessage = response as ErosMessage;
                                emp.MessageSequenceOverride = (responseMessage.sequence - 1) % 16;

                                messageExchange = await MessageExchangeProvider.GetMessageExchanger(messageExchangeParameters, Pod);
                                await messageExchange.InitializeExchange(messageProgress, ct);
                                response = await messageExchange.GetResponse(requestMessage, messageProgress, ct);
                                ErosPod.RuntimeVariables.NonceSync = null;
                                result = messageExchange.ParseResponse(response, Pod);
                                if (ErosPod.RuntimeVariables.NonceSync.HasValue)
                                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Nonce re-negotiation failed");
                            }
                            return result;
                        }
                        catch (Exception e)
                        {
                            return new MessageExchangeResult(e);
                        }
                        finally
                        {
                            ErosRepository.Instance.Save(ErosPod);
                        }
                    });
        }

        private async Task<IMessageExchangeResult> UpdateStatusInternal(IMessageExchangeProgress progress, CancellationToken ct,
            StatusRequestType updateType = StatusRequestType.Standard)
        {
            var request = new ErosMessageBuilder().WithStatus(updateType).Build();
            return await PerformExchange(request, GetStandardParameters(), progress, ct);
        }

        public async Task<IMessageExchangeResult> UpdateStatus(IMessageExchangeProgress progress, CancellationToken ct,
            StatusRequestType updateType = StatusRequestType.Standard)
        {
            try
            {
                Debug.WriteLine($"Updating pod status, request type {updateType}");
                return await this.UpdateStatusInternal(progress, ct, updateType);
            }
            catch (Exception e)
            {
                return new MessageExchangeResult(e);
            }
        }

        public async Task<IMessageExchangeResult> AcknowledgeAlerts(IMessageExchangeProgress progress, CancellationToken ct, byte alertMask)
        {
            try
            {
                Debug.WriteLine($"Acknowledging alerts, bitmask: {alertMask}");
                await UpdateStatusInternal(progress, ct);
                AssertImmediateBolusInactive();
                if (Pod.Status.Progress < PodProgress.PairingSuccess)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod not paired completely yet.");

                if (Pod.Status.Progress == PodProgress.ErrorShuttingDown)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is shutting down, cannot acknowledge alerts.");

                if (Pod.Status.Progress == PodProgress.AlertExpiredShuttingDown)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Acknowledgement period expired, pod is shutting down");

                if (Pod.Status.Progress > PodProgress.AlertExpiredShuttingDown)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is not active");

                if ((Pod.Status.AlertMask & alertMask) != alertMask)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Bitmask is invalid for current alert state");

                var request = new ErosMessageBuilder().WithAcknowledgeAlerts(alertMask).Build();
                return await PerformExchange(request, GetStandardParameters(), progress, ct);
            }
            catch (Exception e)
            {
                return new MessageExchangeResult(e);
            }
        }

        public async Task<IMessageExchangeResult> Bolus(IMessageExchangeProgress progress, CancellationToken ct, decimal bolusAmount)
        {
            try
            {
                Debug.WriteLine($"Bolusing {bolusAmount}U");
                await UpdateStatusInternal(progress, ct);
                AssertRunningStatus();
                AssertImmediateBolusInactive();

                if (bolusAmount < 0.05m)
                    throw new OmniCoreWorkflowException(FailureType.InvalidParameter, "Cannot bolus less than 0.05U");

                if (bolusAmount % 0.05m != 0)
                    throw new OmniCoreWorkflowException(FailureType.InvalidParameter, "Bolus must be multiples of 0.05U");

                if (bolusAmount > 30m)
                    throw new OmniCoreWorkflowException(FailureType.InvalidParameter, "Cannot bolus more than 30U");

                var request = new ErosMessageBuilder().WithBolus(bolusAmount).Build();
                var result = await PerformExchange(request, GetStandardParameters(), progress, ct);

                if (Pod.Status.BolusState != BolusState.Immediate)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not start bolusing");

                return result;

            }
            catch (Exception e)
            {
                return new MessageExchangeResult(e);
            }
        }

        public async Task<IMessageExchangeResult> CancelBolus(IMessageExchangeProgress progress, CancellationToken ct)
        {
            try
            {
                await UpdateStatusInternal(progress, ct);
                AssertRunningStatus();

                if (Pod.Status.BolusState != BolusState.Immediate)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Immediate bolus is not running");

                var request = new ErosMessageBuilder().WithCancelBolus().Build();
                var result = await PerformExchange(request, GetStandardParameters(), progress, ct);

                if (Pod.Status.BolusState == BolusState.Immediate)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Failed to cancel running bolus");

                return result;
            }
            catch (Exception e)
            {
                return new MessageExchangeResult(e);
            }

        }

        public async Task<IMessageExchangeResult> Deactivate(IMessageExchangeProgress progress, CancellationToken ct)
        {
            try
            {
                AssertPaired();

                await UpdateStatusInternal(progress, ct);

                if (Pod.Status.Progress >= PodProgress.Inactive)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod already deactivated");

                var request = new ErosMessageBuilder().WithDeactivate().Build();
                var result = await PerformExchange(request, GetStandardParameters(), progress, ct);

                if (Pod.Status.Progress != PodProgress.Inactive)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Failed to deactivate");

                return result;
            }
            catch (Exception e)
            {
                return new MessageExchangeResult(e);
            }

        }

        private void AssertImmediateBolusInactive()
        {
            if (Pod.Status != null && Pod.Status.BolusState == BolusState.Immediate)
                throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Bolus operation in progress");
        }

        private void AssertPaired()
        {
            if (Pod.Status == null || Pod.Status.Progress < PodProgress.PairingSuccess)
                throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is not paired");
        }

        private void AssertRunningStatus()
        {
            if (Pod.Status == null || Pod.Status.Progress < PodProgress.Running)
                throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is not yet running");

            if (Pod.Status == null || Pod.Status.Progress > PodProgress.RunningLow)
                throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is not running");
        }

    }
}
