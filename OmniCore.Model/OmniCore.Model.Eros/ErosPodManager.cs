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
        readonly SemaphoreSlim ConversationMutex;

        private Nonce nonce;
        public Nonce Nonce
        {
            get
            {
                if (nonce == null)
                {
                    if (Pod?.Lot != null && Pod?.Serial != null)
                        nonce = new Nonce((ErosPod)Pod);
                }
                return nonce;
            }
        }

        public ErosPodManager(ErosPod pod, IMessageExchangeProvider messageExchangeProvider)
        {
            ErosPod = pod;
            MessageExchangeProvider = messageExchangeProvider;
            ConversationMutex = new SemaphoreSlim(1, 1);
        }

        public async Task<IConversation> StartConversation(int timeoutMilliseconds = 0)
        {
            if (timeoutMilliseconds == 0)
            {
                await ConversationMutex.WaitAsync();
            }
            else
            {
                if (!await ConversationMutex.WaitAsync(timeoutMilliseconds))
                    return null;
            }

            return new ErosConversation(ConversationMutex);
        }

        private ErosMessageExchangeParameters GetStandardParameters()
        {
            return new ErosMessageExchangeParameters() { Nonce = Nonce, AllowAutoLevelAdjustment = true };
        }

        private async Task<bool> PerformExchange(IMessage requestMessage, IMessageExchangeParameters messageExchangeParameters,
                    IConversation conversation)
        {
            var emp = messageExchangeParameters as ErosMessageExchangeParameters;
            var progress = conversation.NewExchange();
            try
            {
                progress.Running = true;
                var messageExchange = await MessageExchangeProvider.GetMessageExchange(messageExchangeParameters, Pod);
                await messageExchange.InitializeExchange(progress);
                var response = await messageExchange.GetResponse(requestMessage, progress);
                messageExchange.ParseResponse(response, Pod, progress);

                if (ErosPod.RuntimeVariables.NonceSync.HasValue)
                {
                    var responseMessage = response as ErosMessage;
                    emp.MessageSequenceOverride = (responseMessage.sequence - 1) % 16;
                    messageExchange = await MessageExchangeProvider.GetMessageExchange(messageExchangeParameters, Pod);
                    await messageExchange.InitializeExchange(progress);
                    response = await messageExchange.GetResponse(requestMessage, progress);
                    messageExchange.ParseResponse(response, Pod, progress);
                    if (ErosPod.RuntimeVariables.NonceSync.HasValue)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Nonce re-negotiation failed");
                }
            }
            catch (Exception e)
            {
                progress.SetException(e);
            }
            finally
            {
                ErosRepository.Instance.Save(ErosPod);
                progress.Running = false;
                progress.Finished = true;
            }

            return !conversation.Failed && !conversation.Canceled;
        }

        private async Task<bool> UpdateStatusInternal(IConversation conversation,
            StatusRequestType updateType = StatusRequestType.Standard)
        {
            var request = new ErosMessageBuilder().WithStatus(updateType).Build();
            return await PerformExchange(request, GetStandardParameters(), conversation);
        }

        public async Task UpdateStatus(IConversation conversation, 
            StatusRequestType updateType = StatusRequestType.Standard)
        {
            try
            {
                await this.UpdateStatusInternal(conversation, updateType);
            }
            catch (Exception e)
            {
                conversation.Exception = e;
            }
        }

        public async Task AcknowledgeAlerts(IConversation conversation, byte alertMask)
        {
            try
            {
                Debug.WriteLine($"Acknowledging alerts, bitmask: {alertMask}");
                if (!await UpdateStatusInternal(conversation))
                    return;

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
                if (!await PerformExchange(request, GetStandardParameters(), conversation))
                    return;

                if ((Pod.Status.AlertMask & alertMask) != 0)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Alerts not completely acknowledged");

            }
            catch (Exception e)
            {
                conversation.Exception = e;
            }
        }

        public async Task SetTempBasal(IConversation conversation, decimal basalRate, decimal durationInHours)
        {
            try
            {
                // progress.CommandText = $"Set Temp Basal {basalRate}U/h for {durationInHours}h";
                if (!await UpdateStatusInternal(conversation))
                    return;

                AssertRunningStatus();
                AssertImmediateBolusInactive();

                var request = new ErosMessageBuilder().WithTempBasal(basalRate, durationInHours).Build();
                if (!await PerformExchange(request, GetStandardParameters(), conversation))
                    return;

                if (Pod.Status.BasalState != BasalState.Temporary)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not start the temp basal");
            }
            catch (Exception e)
            {
                conversation.Exception = e;
            }
        }

        public async Task Bolus(IConversation conversation, decimal bolusAmount)
        {
            try
            {
                //progress.CommandText = $"Bolusing {bolusAmount}U";
                Debug.WriteLine($"Bolusing {bolusAmount}U");
                if (!await UpdateStatusInternal(conversation))
                    return;

                AssertRunningStatus();
                AssertImmediateBolusInactive();

                if (bolusAmount < 0.05m)
                    throw new OmniCoreWorkflowException(FailureType.InvalidParameter, "Cannot bolus less than 0.05U");

                if (bolusAmount % 0.05m != 0)
                    throw new OmniCoreWorkflowException(FailureType.InvalidParameter, "Bolus must be multiples of 0.05U");

                if (bolusAmount > 30m)
                    throw new OmniCoreWorkflowException(FailureType.InvalidParameter, "Cannot bolus more than 30U");

                var request = new ErosMessageBuilder().WithBolus(bolusAmount).Build();
                if (!await PerformExchange(request, GetStandardParameters(), conversation))
                    return;

                if (Pod.Status.BolusState != BolusState.Immediate)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not start bolusing");

                while (Pod.Status.BolusState == BolusState.Immediate)
                {
                    var tickCount = (int)(Pod.Status.NotDeliveredInsulin / 0.05m);
                    await Task.Delay(tickCount * 2000 + 500, conversation.Token);

                    if (conversation.Token.IsCancellationRequested)
                    {
                        var cancelRequest = new ErosMessageBuilder().WithCancelBolus().Build();
                        var cancelResult = await PerformExchange(request, GetStandardParameters(), conversation);

                        if (!cancelResult || Pod.Status.BolusState == BolusState.Immediate)
                        {
                            conversation.CancelFailed();
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (!await UpdateStatusInternal(conversation))
                        return;
                }

                if (conversation.Canceled || conversation.Failed)
                    return;

                if (Pod.Status.NotDeliveredInsulin != 0)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Not all insulin was delivered");
            }
            catch (Exception e)
            {
                conversation.Exception = e;
            }
        }

        public async Task Deactivate(IConversation conversation)
        {
            try
            {
                // progress.CommandText = $"Deactivating Pod";
                AssertPaired();

                if (!await UpdateStatusInternal(conversation))
                    return;

                if (Pod.Status.Progress >= PodProgress.Inactive)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod already deactivated");

                var request = new ErosMessageBuilder().WithDeactivate().Build();
                if (!await PerformExchange(request, GetStandardParameters(), conversation))
                    return;

                if (Pod.Status.Progress != PodProgress.Inactive)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Failed to deactivate");
            }
            catch (Exception e)
            {
                conversation.Exception = e;
            }
        }

        public async Task Pair(IConversation conversation, int utcOffsetMinutes)
        {
            try
            {
                // progress.CommandText = $"Pairing with Pod";

                AssertNotPaired();

                if (Pod.Status == null || Pod.Status.Progress <= PodProgress.TankFillCompleted)
                {
                    var parameters = GetStandardParameters();
                    parameters.AddressOverride = 0xffffffff;
                    parameters.AckAddressOverride = Pod.RadioAddress;
                    parameters.TransmissionLevelOverride = TxPower.A3_BelowNormal;
                    parameters.AllowAutoLevelAdjustment = false;

                    var request = new ErosMessageBuilder().WithAssignAddress(Pod.RadioAddress).Build();
                    if (!await PerformExchange(request, parameters, conversation))
                        return;

                    if (Pod.Status == null)
                        throw new OmniCoreWorkflowException(FailureType.RadioRecvTimeout, "Pod did not respond to pairing request");
                    else if (Pod.Status.Progress < PodProgress.TankFillCompleted)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod is not filled with enough insulin for activation.");

                }

                if (Pod.Status != null && Pod.Status.Progress < PodProgress.PairingSuccess)
                {
                    Pod.ActivationDate = DateTime.UtcNow;
                    var podDate = Pod.ActivationDate.Value + TimeSpan.FromMinutes(utcOffsetMinutes);
                    var parameters = GetStandardParameters();
                    parameters.AddressOverride = 0xffffffff;
                    parameters.AckAddressOverride = Pod.RadioAddress;
                    parameters.TransmissionLevelOverride = TxPower.A3_BelowNormal;
                    parameters.MessageSequenceOverride = 1;
                    parameters.AllowAutoLevelAdjustment = false;

                    var request = new ErosMessageBuilder().WithSetupPod(Pod.Lot.Value, Pod.Serial.Value, Pod.RadioAddress,
                        podDate.Year, (byte)podDate.Month, (byte)podDate.Day,
                        (byte)podDate.Hour, (byte)podDate.Minute).Build();

                    if (!await PerformExchange(request, parameters, conversation))
                        return;

                    AssertPaired();
                }
                throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand);
            }
            catch (Exception e)
            {
                conversation.Exception = e;
            }
        }

        public async Task Activate(IConversation conversation)
        {
            try
            {
                // progress.CommandText = $"Activating Pod";
                if (!await UpdateStatusInternal(conversation))
                    return;

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

                    if (!await PerformExchange(request, parameters, conversation))
                        return;

                    request = new ErosMessageBuilder().WithDeliveryFlags(0, 0).Build();
                    if (!await PerformExchange(request, parameters, conversation))
                        return;

                    request = new ErosMessageBuilder().WithPrimeCannula().Build();
                    if (!await PerformExchange(request, parameters, conversation))
                        return;

                    if (Pod.Status.Progress != PodProgress.Purging)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not start priming");
                }

                while (Pod.Status.Progress == PodProgress.Purging)
                {
                    var ticks = (int)(Pod.Status.NotDeliveredInsulin / 0.05m);
                    await Task.Delay(ticks * 1000 + 200);

                    if (!await UpdateStatusInternal(conversation))
                        return;
                }

                if (Pod.Status.Progress != PodProgress.ReadyForInjection)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not reach ready for injection state.");

                if (Pod.UserSettings?.ExpiryWarningAtMinute != null)
                {
                    //TODO: expiry warning
                }
            }
            catch (Exception e)
            {
                conversation.Exception = e;
            }
        }

        public async Task InjectAndStart(IConversation conversation, decimal[] basalSchedule, int utcOffsetInMinutes)
        {
            try
            {
                // progress.CommandText = $"Starting Pod";
                if (!await UpdateStatusInternal(conversation))
                    return;

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
                    if (!await PerformExchange(request, parameters, conversation))
                        return;

                    if (Pod.Status.Progress != PodProgress.BasalScheduleSet)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not acknowledge basal schedule");
                }

                if (Pod.Status.Progress == PodProgress.BasalScheduleSet)
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
                    if (!await PerformExchange(request, GetStandardParameters(), conversation))
                        return;

                    request = new ErosMessageBuilder().WithInsertCannula().Build();
                    if (!await PerformExchange(request, GetStandardParameters(), conversation))
                        return;

                    if (Pod.Status.Progress != PodProgress.Priming)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not start priming the cannula for insertion");

                    Pod.InsertionDate = DateTime.UtcNow;
                }

                while (Pod.Status.Progress == PodProgress.Priming)
                {
                    var ticks = (int)(Pod.Status.NotDeliveredInsulin / 0.05m);
                    await Task.Delay(ticks * 1000 + 200);

                    if (!await UpdateStatusInternal(conversation))
                        return;
                }

                if (Pod.Status.Progress != PodProgress.Running)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not enter the running state");
            }
            catch (Exception e)
            {
                conversation.Exception = e;
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

        public Task ConfigureAlerts(IConversation conversation, AlertConfiguration[] alertConfigurations)
        {
            throw new NotImplementedException();
        }

        public Task CancelBolus(IConversation conversation)
        {
            throw new NotImplementedException();
        }

        public Task CancelTempBasal(IConversation conversation)
        {
            throw new NotImplementedException();
        }

        public Task StartExtendedBolus(IConversation conversation, decimal bolusAmount, decimal durationInHours)
        {
            throw new NotImplementedException();
        }

        public Task CancelExtendedBolus(IConversation conversation)
        {
            throw new NotImplementedException();
        }

        public Task SetBasalSchedule(IConversation conversation, decimal[] schedule, int utcOffsetInMinutes)
        {
            throw new NotImplementedException();
        }

        public Task SuspendBasal(IConversation conversation)
        {
            throw new NotImplementedException();
        }
    }
}
