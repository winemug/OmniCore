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
        readonly ErosMessageExchangeParameters StandardParameters;
        readonly Task MessageExchangeTask;

        public ErosPodManager(ErosPod pod, IMessageExchangeProvider messageExchangeProvider)
        {
            StandardParameters = new ErosMessageExchangeParameters() { };
            ErosPod = pod;
            MessageExchangeProvider = messageExchangeProvider;
            MessageExchangeTask = Task.Run(() => { });
        }

        private async Task<IMessageExchangeResult> PerformExchange(IMessage requestMessage, IMessageExchangeParameters messageExchangeParameters,
                    IMessageProgress messageProgress, CancellationToken ct)
        {
            return await await MessageExchangeTask.ContinueWith(
                    async (IMessageExchangeResult) =>
                    {
                        try
                        {
                            var messageExchange = await MessageExchangeProvider.GetMessageExchanger(messageExchangeParameters, Pod, messageProgress, ct).ConfigureAwait(false);
                            var response = await messageExchange.GetResponse(requestMessage, messageProgress, ct);
                            return messageExchange.ParseResponse(response, Pod);
                        }
                        catch (Exception e)
                        {
                            return new MessageExchangeResult(false, e);
                        }
                        finally
                        {
                            ErosRepository.Instance.Save(ErosPod);
                        }
                    });
        }

        private async Task<IMessageExchangeResult> UpdateStatusInternal(IMessageProgress progress, CancellationToken ct,
            StatusRequestType update_type = StatusRequestType.Standard)
        {
            var request = new ErosMessageBuilder().WithStatus(update_type).Build();
            return await PerformExchange(request, StandardParameters, progress, ct);
        }

        public async Task UpdateStatus(IMessageProgress progress, CancellationToken ct,
            StatusRequestType update_type = StatusRequestType.Standard)
        {
            try
            {
                Debug.WriteLine($"Updating pod status, request type {update_type}");
                await this.UpdateStatusInternal(progress, ct, update_type);
            }
            catch (OmniCoreException) { throw; }
            catch (Exception e)
            {
                throw new PdmException("Unexpected error", e);
            }
        }

        public async Task AcknowledgeAlerts(IMessageProgress progress, CancellationToken ct, byte alert_mask)
        {
            try
            {
                Debug.WriteLine($"Acknowledging alerts, bitmask: {alert_mask}");
                await UpdateStatusInternal(progress, ct);
                AssertImmediateBolusInactive();
                if (Pod.Status.Progress < PodProgress.PairingSuccess)
                    throw new PdmException("Pod not paired completely yet.");

                if (Pod.Status.Progress == PodProgress.ErrorShuttingDown)
                    throw new PdmException("Pod is shutting down, cannot acknowledge alerts.");

                if (Pod.Status.Progress == PodProgress.AlertExpiredShuttingDown)
                    throw new PdmException("Acknowledgement period expired, pod is shutting down");

                if (Pod.Status.Progress > PodProgress.AlertExpiredShuttingDown)
                    throw new PdmException("Pod is not active");

                if ((Pod.Status.AlertMask & alert_mask) != alert_mask)
                    throw new PdmException("Bitmask is invalid for current alert state");

                // await send_request(ProtocolHelper.request_acknowledge_alerts(alert_mask));
            }
            catch (OmniCoreException) { throw; }
            catch (Exception e)
            {
                throw new PdmException("Unexpected error", e);
            }
        }

        public async Task Bolus(IMessageProgress progress, CancellationToken ct, decimal bolusAmount)
        {
            try
            {
                Debug.WriteLine($"Bolusing {bolusAmount}U");
                await UpdateStatusInternal(progress, ct);
                AssertRunningStatus();
                AssertImmediateBolusInactive();

                if (bolusAmount < 0.05m)
                    throw new PdmException("Cannot bolus less than 0.05U");

                if (bolusAmount % 0.05m != 0)
                    throw new PdmException("Bolus must be multiples of 0.05U");

                if (bolusAmount > 30m)
                    throw new PdmException("Cannot bolus more than 30U");

                // await send_request(ProtocolHelper.request_bolus(bolusAmount), true);

                if (Pod.Status.BolusState != BolusState.Immediate)
                    throw new PdmException("Pod did not start bolusing");

            }
            catch (OmniCoreException) { throw; }
            catch (Exception e)
            {
                throw new PdmException("Unexpected error", e);
            }
        }

        public async Task CancelBolus(IMessageProgress progress, CancellationToken ct)
        {
            try
            {
                await UpdateStatusInternal(progress, ct);
                AssertRunningStatus();

                if (Pod.Status.BolusState != BolusState.Immediate)
                    throw new PdmException("Immediate bolus is not running");

                // await send_request(ProtocolHelper.request_cancel_bolus(), true);

                if (Pod.Status.BolusState == BolusState.Immediate)
                    throw new PdmException("Failed to cancel running bolus");

            }
            catch (OmniCoreException) { throw; }
            catch (Exception e)
            {
                throw new PdmException("Unexpected error", e);
            }
        }

        private void AssertImmediateBolusInactive()
        {
            if (Pod.Status.BolusState == BolusState.Immediate)
                throw new PdmException("Bolus operation in progress");
        }

        private void AssertRunningStatus()
        {
            if (Pod.Status.Progress < PodProgress.Running)
                throw new PdmException("Pod is not yet running");

            if (Pod.Status.Progress > PodProgress.RunningLow)
                throw new PdmException("Pod is not running");
        }

    }
}
