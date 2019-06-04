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
        private Nonce nonce;
        public Nonce Nonce
        {
            get
            {
                if (nonce == null)
                {
                    if (Pod.Lot.HasValue && Pod.Serial.HasValue)
                        nonce = new Nonce((ErosPod)Pod);
                }
                return nonce;
            }
        }

    public ErosPodManager(ErosPod pod, IMessageExchangeProvider messageExchangeProvider)
        {
            ErosPod = pod;
            MessageExchangeProvider = messageExchangeProvider;
            MessageExchangeTask = Task.Run(() => { });
        }

        private ErosMessageExchangeParameters GetStandardParameters()
        {
            return new ErosMessageExchangeParameters() { Nonce = Nonce };
        }

        private async Task<IMessageExchangeResult> PerformExchange(IMessage requestMessage, IMessageExchangeParameters messageExchangeParameters,
                    IMessageExchangeProgress progress)
        {
            progress.ActionStatusText = "Waiting in queue";
            var emp = messageExchangeParameters as ErosMessageExchangeParameters;
            return await await MessageExchangeTask.ContinueWith(
                    async (IMessageExchangeResult) =>
                    {
                        try
                        {
                            var messageExchange = await MessageExchangeProvider.GetMessageExchanger(messageExchangeParameters, Pod);
                            await messageExchange.InitializeExchange(progress);
                            var response = await messageExchange.GetResponse(requestMessage, progress);
                            var result = messageExchange.ParseResponse(response, Pod);

                            if (result.Success && ErosPod.RuntimeVariables.NonceSync.HasValue)
                            {
                                var responseMessage = response as ErosMessage;
                                emp.MessageSequenceOverride = (responseMessage.sequence - 1) % 16;
                                messageExchange = await MessageExchangeProvider.GetMessageExchanger(messageExchangeParameters, Pod);
                                await messageExchange.InitializeExchange(progress);
                                response = await messageExchange.GetResponse(requestMessage, progress);
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

        private async Task<IMessageExchangeResult> UpdateStatusInternal(IMessageExchangeProgress progress,
            StatusRequestType updateType = StatusRequestType.Standard)
        {
            progress.ActionText = "Running status update";
            var request = new ErosMessageBuilder().WithStatus(updateType).Build();
            return await PerformExchange(request, GetStandardParameters(), progress);
        }

        public async Task<IMessageExchangeResult> UpdateStatus(IMessageExchangeProgress progress, 
            StatusRequestType updateType = StatusRequestType.Standard)
        {
            try
            {
                progress.CommandText = "Updating Status";
                Debug.WriteLine($"Updating pod status, request type {updateType}");
                return await this.UpdateStatusInternal(progress, updateType);
            }
            catch (Exception e)
            {
                return new MessageExchangeResult(e);
            }
        }

        public async Task<IMessageExchangeResult> AcknowledgeAlerts(IMessageExchangeProgress progress, byte alertMask)
        {
            try
            {
                progress.CommandText = "Acknowledging Alerts";
                Debug.WriteLine($"Acknowledging alerts, bitmask: {alertMask}");
                var result = await UpdateStatusInternal(progress);
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
                result = await PerformExchange(request, GetStandardParameters(), progress);
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

        public async Task<IMessageExchangeResult> SetTempBasal(IMessageExchangeProgress progress, decimal basalRate, decimal durationInHours)
        {
            try
            {
                progress.CommandText = $"Set Temp Basal {basalRate}U/h for {durationInHours}h";
                await UpdateStatusInternal(progress);
                AssertRunningStatus();
                AssertImmediateBolusInactive();

                var request = new ErosMessageBuilder().WithTempBasal(basalRate, durationInHours).Build();
                var result = await PerformExchange(request, GetStandardParameters(), progress);

                if (Pod.Status.BasalState != BasalState.Temporary)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not start the temp basal");

                return result;
            }
            catch (Exception e)
            {
                return new MessageExchangeResult(e);
            }
        }

        public async Task<IMessageExchangeResult> Bolus(IMessageExchangeProgress progress, decimal bolusAmount)
        {
            try
            {
                progress.CommandText = $"Bolusing {bolusAmount}U";
                Debug.WriteLine($"Bolusing {bolusAmount}U");
                await UpdateStatusInternal(progress);
                AssertRunningStatus();
                AssertImmediateBolusInactive();

                if (bolusAmount < 0.05m)
                    throw new OmniCoreWorkflowException(FailureType.InvalidParameter, "Cannot bolus less than 0.05U");

                if (bolusAmount % 0.05m != 0)
                    throw new OmniCoreWorkflowException(FailureType.InvalidParameter, "Bolus must be multiples of 0.05U");

                if (bolusAmount > 30m)
                    throw new OmniCoreWorkflowException(FailureType.InvalidParameter, "Cannot bolus more than 30U");

                var request = new ErosMessageBuilder().WithBolus(bolusAmount).Build();
                var result = await PerformExchange(request, GetStandardParameters(), progress);

                if (Pod.Status.BolusState != BolusState.Immediate)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not start bolusing");

                var tickCount = (int)(bolusAmount / 0.05m);

                await Task.Delay(tickCount * 2000 + 500, progress.Token);

                if (progress.Token.IsCancellationRequested)
                {
                    var cancelRequest = new ErosMessageBuilder().WithCancelBolus().Build();
                    var cancelResult = await PerformExchange(request, GetStandardParameters(), progress);

                    if (!cancelResult.Success || Pod.Status.BolusState == BolusState.Immediate)
                    {
                        progress.CancelFailed();
                        return cancelResult;
                    }

                }
                else
                {
                    result = await UpdateStatusInternal(progress);
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

        public async Task<IMessageExchangeResult> Deactivate(IMessageExchangeProgress progress)
        {
            try
            {
                progress.CommandText = $"Deactivating Pod";
                AssertPaired();

                await UpdateStatusInternal(progress);

                if (Pod.Status.Progress >= PodProgress.Inactive)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod already deactivated");

                var request = new ErosMessageBuilder().WithDeactivate().Build();
                var result = await PerformExchange(request, GetStandardParameters(), progress);

                if (Pod.Status.Progress != PodProgress.Inactive)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Failed to deactivate");

                return result;
            }
            catch (Exception e)
            {
                return new MessageExchangeResult(e);
            }
        }

        public async Task<IMessageExchangeResult> Pair(IMessageExchangeProgress progress, int utcOffsetMinutes)
        {
            try
            {
                progress.CommandText = $"Pairing with Pod";
                AssertNotPaired();

                if (Pod.Status == null || Pod.Status.Progress <= PodProgress.TankFillCompleted)
                {
                    var parameters = GetStandardParameters();
                    parameters.AddressOverride = 0xffffffff;
                    parameters.AckAddressOverride = Pod.RadioAddress;
                    parameters.TransmissionLevelOverride = TxPower.Low;

                    var request = new ErosMessageBuilder().WithAssignAddress(Pod.RadioAddress).Build();
                    var result = await PerformExchange(request, parameters, progress);

                    if (!result.Success && result.FailureType != FailureType.AlreadyExecuted)
                    {
                        return result;
                    }

                    if (Pod.Status == null)
                        throw new OmniCoreWorkflowException(FailureType.RadioRecvTimeout, "Pod did not respond to pairing request");
                    //else if (Pod.Status.Progress < PodProgress.TankFillCompleted)
                    //    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod is not filled with enough insulin for activation.");
                }

                if (Pod.Status != null && Pod.Status.Progress < PodProgress.PairingSuccess)
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

                    var result = await PerformExchange(request, parameters, progress);

                    if (!result.Success)
                    {
                        if (result.FailureType == FailureType.AlreadyExecuted)
                        {
                            var updateStatusResult = await UpdateStatusInternal(progress);
                            if (updateStatusResult.Success && Pod.Status.Progress >= PodProgress.PairingSuccess)
                                return updateStatusResult;
                        }
                        return result;
                    }

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

        public async Task<IMessageExchangeResult> Activate(IMessageExchangeProgress progress)
        {
            try
            {
                progress.CommandText = $"Activating Pod";
                var result = await UpdateStatusInternal(progress);
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

                    result = await PerformExchange(request, parameters, progress);
                    if (!result.Success && result.FailureType != FailureType.AlreadyExecuted)
                        return result;

                    //request = new ErosMessageBuilder().WithDeliveryFlags(0, 0).Build();
                    //result = await PerformExchange(request, parameters, progress);
                    //if (!result.Success && result.FailureType != FailureType.AlreadyExecuted)
                    //    return result;

                    request = new ErosMessageBuilder().WithPrimeCannula().Build();
                    result = await PerformExchange(request, parameters, progress);
                    if (!result.Success)
                    {
                        if (result.FailureType == FailureType.AlreadyExecuted)
                        {
                            result = await UpdateStatusInternal(progress);
                            if (result.Success)
                            {
                                result = await PerformExchange(request, parameters, progress);
                            }
                        }
                        return result;
                    }
                }

                if (Pod.Status.Progress == PodProgress.Purging)
                {
                    var ticks = 52;
                    if (Pod.Status != null)
                        ticks = (int)(Pod.Status.NotDeliveredInsulin / 0.05m);
                    await Task.Delay(ticks * 1000 + 500);
                }

                while (Pod.Status.Progress == PodProgress.Purging)
                {
                    result = await UpdateStatusInternal(progress);
                    var ticks = (int)(Pod.Status.NotDeliveredInsulin / 0.05m);
                    await Task.Delay(ticks * 1000 + 500);
                }

                if (Pod.Status.Progress != PodProgress.ReadyForInjection)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not reach ready for injection state.");

                if (Pod.UserSettings?.ExpiryWarningAtMinute != null)
                {
                    //TODO: expiry warning
                }
                return new MessageExchangeResult();
            }
            catch (Exception e)
            {
                return new MessageExchangeResult(e);
            }
        }

        public async Task<IMessageExchangeResult> InjectAndStart(IMessageExchangeProgress progress, decimal[] basalSchedule, int utcOffsetInMinutes)
        {
            try
            {
                progress.CommandText = $"Starting Pod";
                var result = await UpdateStatusInternal(progress);
                if (!result.Success)
                    return result;

                if (Pod.Status.Progress >= PodProgress.Running)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is already started");

                if (Pod.Status.Progress < PodProgress.ReadyForInjection)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is not ready for injection");

                if (Pod.Status.Progress == PodProgress.ReadyForInjection)
                {
                    AssertBasalScheduleValid(basalSchedule);

                    var podDate = DateTime.UtcNow + TimeSpan.FromMinutes(utcOffsetInMinutes);
                    var parameters = GetStandardParameters();
                    parameters.RepeatFirstPacket = true;
                    parameters.CriticalWithFollowupRequired = true;

                    var request = new ErosMessageBuilder()
                        .WithBasalSchedule(basalSchedule, (ushort)podDate.Hour, (ushort)podDate.Minute, (ushort)podDate.Second)
                        .Build();
                    result = await PerformExchange(request, parameters, progress);

                    if (!result.Success)
                        return result;

                    if (Pod.Status.Progress != PodProgress.BasalScheduleSet)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not acknowledge basal schedule");
                }

                if (Pod.Status.Progress != PodProgress.BasalScheduleSet)
                {
                    var acs = new List<AlertConfiguration>(new[]
                    {
                        new AlertConfiguration()
                        {
                            activate = false,
                            alert_index = 7,
                            alert_duration = 0,
                            alert_after_minutes = 0,
                            beep_type = BeepType.NoSound,
                            beep_repeat_type = BeepPattern.Once
                        },

                        new AlertConfiguration()
                        {
                            activate = false,
                            alert_index = 0,
                            alert_duration = 0,
                            alert_after_minutes = 15,
                            trigger_auto_off = true,
                            beep_type = BeepType.BipBeepFourTimes,
                            beep_repeat_type = BeepPattern.OnceEveryMinuteForFifteenMinutes
                        }

                    });

                    var request = new ErosMessageBuilder().WithAlertSetup(acs).Build();
                    result = await PerformExchange(request, GetStandardParameters(), progress);
                    if (!result.Success)
                        return result;

                    request = new ErosMessageBuilder().WithInsertCannula().Build();
                    result = await PerformExchange(request, GetStandardParameters(), progress);
                    if (!result.Success)
                        return result;

                    if (Pod.Status.Progress != PodProgress.Priming)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not start priming the cannula for insertion");

                    Pod.InsertionDate = DateTime.UtcNow;
                }

                if (Pod.Status.Progress == PodProgress.Priming)
                {
                    var waitTask = Task.Delay(10500, progress.Token);
                    await waitTask;
                    result = await UpdateStatusInternal(progress);
                    if (!result.Success)
                        return result;

                    if (Pod.Status.Progress != PodProgress.Running)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not enter the running state");
                }
                return result;
            }
            catch (Exception e)
            {
                return new MessageExchangeResult(e);
            }
        }

        private void AssertBasalScheduleValid(decimal[] basalSchedule)
        {
            if (basalSchedule.Length != 48)
                throw new OmniCoreWorkflowException(FailureType.InvalidParameter, "Invalid basal schedule, it must contain 48 half hour elements.");

            foreach(var entry in basalSchedule)
            {
                if (entry % 0.05m != 0)
                    throw new OmniCoreWorkflowException(FailureType.InvalidParameter, "Basal schedule entries must be multiples of 0.05U");

                if (entry < 0.05m)
                    throw new OmniCoreWorkflowException(FailureType.InvalidParameter, "Basal schedule entry cannot be less than 0.05U");

                if (entry > 30m)
                    throw new OmniCoreWorkflowException(FailureType.InvalidParameter, "Basal schedule entry cannot be more than 30U");
            }
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

        public Task<IMessageExchangeResult> ConfigureAlerts(IMessageExchangeProgress progress, AlertConfiguration[] alertConfigurations)
        {
            throw new NotImplementedException();
        }

        public Task<IMessageExchangeResult> CancelBolus(IMessageExchangeProgress progress)
        {
            throw new NotImplementedException();
        }

        public Task<IMessageExchangeResult> CancelTempBasal(IMessageExchangeProgress progress)
        {
            throw new NotImplementedException();
        }

        public Task<IMessageExchangeResult> StartExtendedBolus(IMessageExchangeProgress progress, decimal bolusAmount, decimal durationInHours)
        {
            throw new NotImplementedException();
        }

        public Task<IMessageExchangeResult> CancelExtendedBolus(IMessageExchangeProgress progress)
        {
            throw new NotImplementedException();
        }

        public Task<IMessageExchangeResult> SetBasalSchedule(IMessageExchangeProgress progress, decimal[] schedule, int utcOffsetInMinutes)
        {
            throw new NotImplementedException();
        }

        public Task<IMessageExchangeResult> SuspendBasal(IMessageExchangeProgress progress)
        {
            throw new NotImplementedException();
        }
    }
}
