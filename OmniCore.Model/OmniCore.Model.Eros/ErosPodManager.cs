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
                var result = await UpdateStatusInternal(progress, ct);
                if (!result.Success)
                    return result;

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
                result = await PerformExchange(request, GetStandardParameters(), progress, ct);
                if (!result.Success)
                    return result;

                if ((Pod.Status.AlertMask & alertMask) != 0)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Alerts not completely acknowledged");

                return result;
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

                var tickCount = (int)(bolusAmount / 0.05m);

                await Task.Delay(tickCount * 2000 + 500, ct);

                if (ct.IsCancellationRequested)
                {
                    var cancelRequest = new ErosMessageBuilder().WithCancelBolus().Build();
                    var cancelResult = await PerformExchange(request, GetStandardParameters(), progress, ct);

                    if (!cancelResult.Success)
                        return cancelResult;

                    if (Pod.Status.BolusState == BolusState.Immediate)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Failed to cancel running bolus");
                }
                else
                {
                    result = await UpdateStatusInternal(progress, ct);
                    if (Pod.Status.NotDeliveredInsulin != 0)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Not all insulin was delivered");
                }
                return result;
            }
            catch (Exception e)
            {
                return new MessageExchangeResult(e);
            }
        }

        //public async Task<IMessageExchangeResult> CancelBolus(IMessageExchangeProgress progress, CancellationToken ct)
        //{
        //    try
        //    {
        //        await UpdateStatusInternal(progress, ct);
        //        AssertRunningStatus();

        //        if (Pod.Status.BolusState != BolusState.Immediate)
        //            throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Immediate bolus is not running");

        //        var request = new ErosMessageBuilder().WithCancelBolus().Build();
        //        var result = await PerformExchange(request, GetStandardParameters(), progress, ct);

        //        if (Pod.Status.BolusState == BolusState.Immediate)
        //            throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Failed to cancel running bolus");

        //        return result;
        //    }
        //    catch (Exception e)
        //    {
        //        return new MessageExchangeResult(e);
        //    }

        //}

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

        public async Task<IMessageExchangeResult> Pair(IMessageExchangeProgress progress, CancellationToken ct, int utcOffsetMinutes)
        {
            try
            {
                AssertNotPaired();

                if (Pod.Status == null || Pod.Status.Progress <= PodProgress.TankFillCompleted)
                {
                    var parameters = GetStandardParameters();
                    parameters.AddressOverride = 0xffffffff;
                    parameters.AckAddressOverride = Pod.RadioAddress;
                    parameters.TransmissionLevelOverride = TxPower.Low;

                    var request = new ErosMessageBuilder().WithAssignAddress(Pod.RadioAddress).Build();
                    var result = await PerformExchange(request, parameters, progress, ct);

                    if (!result.Success)
                        return result;

                    if (Pod.Status == null)
                        throw new OmniCoreWorkflowException(FailureType.PodUnreachable, "Pod did not respond to pairing request");
                    else if (Pod.Status.Progress < PodProgress.TankFillCompleted)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod is not filled with enough insulin for activation.");
                }

                if (Pod.Status != null && Pod.Status.Progress == PodProgress.TankFillCompleted)
                {
                    Pod.ActivationDate = DateTime.UtcNow;
                    var podDate = Pod.ActivationDate.Value + TimeSpan.FromMinutes(utcOffsetMinutes);
                    var parameters = GetStandardParameters();
                    parameters.AddressOverride = 0xffffffff;
                    parameters.AckAddressOverride = Pod.RadioAddress;
                    parameters.TransmissionLevelOverride = TxPower.Normal;
                    parameters.MessageSequenceOverride = 1;

                    var request = new ErosMessageBuilder().WithSetupPod(Pod.Lot.Value, Pod.Serial.Value, Pod.RadioAddress,
                        podDate.Year, (byte)podDate.Month, (byte)podDate.Day,
                        (byte)podDate.Hour, (byte)podDate.Minute).Build();

                    var result = await PerformExchange(request, parameters, progress, ct);

                    if (!result.Success)
                        return result;

                    AssertPaired();

                    return result;
                }
                throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand);
            }
            catch (Exception e)
            {
                return new MessageExchangeResult(e);
            }
        }

        public async Task<IMessageExchangeResult> Activate(IMessageExchangeProgress progress, CancellationToken ct)
        {
            try
            {
                var result = await UpdateStatusInternal(progress, ct);
                if (!result.Success)
                    return result;

                if (Pod.Status.Progress > PodProgress.ReadyForInjection)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is already activated");

                if (Pod.Status.Progress == PodProgress.PairingSuccess)
                {
                    var parameters = GetStandardParameters();
                    parameters.MessageSequenceOverride = 2;

                    var ac = new AlertConfiguration
                    {
                        activate = true,
                        alert_index = 7,
                        alert_after_minutes = 5,
                        alert_duration = 55,
                        beep_type = BeepType.BipBeepFourTimes,
                        beep_repeat_type = BeepPattern.OnceEveryFiveMinutes
                    };

                    var request = new ErosMessageBuilder()
                        .WithAlertSetup(new List<AlertConfiguration>(new[] { ac }))
                        .Build();

                    result = await PerformExchange(request, parameters, progress, ct);
                    if (!result.Success)
                        return result;

                    request = new ErosMessageBuilder().WithDeliveryFlags(0, 0).Build();
                    result = await PerformExchange(request, parameters, progress, ct);
                    if (!result.Success)
                        return result;

                    request = new ErosMessageBuilder().WithPrimeCannula().Build();
                    result = await PerformExchange(request, parameters, progress, ct);
                    if (!result.Success)
                        return result;

                    await Task.Delay(57000, ct);

                    if (ct.IsCancellationRequested)
                    {
                        request = new ErosMessageBuilder().WithCancelBolus().Build();
                        return await PerformExchange(request, parameters, progress, ct);
                    }
                    else
                    {
                        if (Pod.Status.Progress != PodProgress.ReadyForInjection)
                            throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not reach ready for injection state.");

                        if (Pod.UserSettings?.ExpiryWarningAtMinute != null)
                        {
                            //TODO: expiry warning
                        }
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                return new MessageExchangeResult(e);
            }
        }

        public async Task<IMessageExchangeResult> Inject(IMessageExchangeProgress progress, CancellationToken ct)
        {
            //TODO:
            return null;
        }

        private void AssertImmediateBolusInactive()
        {
            if (Pod.Status != null && Pod.Status.BolusState == BolusState.Immediate)
                throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Bolus operation in progress");
        }

        private void AssertNotPaired()
        {
            if (Pod.Status != null && Pod.Status.Progress >= PodProgress.PairingSuccess)
                throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is already paired");
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
