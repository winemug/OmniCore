using OmniCore.Mobile.Base;
using OmniCore.Mobile.Base.Interfaces;
using OmniCore.Model.Enums;
using OmniCore.Model.Eros.Data;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Utilities;
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

        public IPodManager Direct { get => this; }

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

        public async Task<IConversation> StartConversation(string intent, int timeoutMilliseconds = 0, RequestSource source = RequestSource.OmniCoreUser)
        {
            IWakeLock wakeLock = null;
            try
            {
                wakeLock = OmniCoreServices.Application.NewBluetoothWakeLock(
                    Guid.NewGuid().ToString()
                    .Replace('-', '_')
                    .Replace('{', '_')
                    .Replace('}', '_')
                    );

                var ret = await wakeLock.Acquire(10000);
                if (!ret)
                {
                    wakeLock.Release();
                    throw new OmniCoreException(FailureType.WakeLockNotAcquired);
                }

                if (timeoutMilliseconds == 0)
                {
                    await ConversationMutex.WaitAsync();
                }
                else
                {
                    if (!await ConversationMutex.WaitAsync(timeoutMilliseconds))
                        throw new OmniCoreTimeoutException(FailureType.OperationInProgress, "Timed out waiting for existing operation to complete");
                }

                Pod.ActiveConversation = new ErosConversation(ConversationMutex, wakeLock, Pod) { RequestSource = source, Intent = intent };
                return Pod.ActiveConversation;
            }
            catch
            {
                wakeLock?.Dispose();
                throw;
            }
        }

        private ErosMessageExchangeParameters GetStandardParameters()
        {
            return new ErosMessageExchangeParameters() { Nonce = Nonce, AllowAutoLevelAdjustment = true };
        }

        private async Task<bool> PerformExchange(IMessage requestMessage, IMessageExchangeParameters messageExchangeParameters,
            IConversation conversation)
        {
            int retries = 0;
            while(retries < 2)
            {
                try
                {
                    var ret = await PerformExchangeInternal(requestMessage, messageExchangeParameters, conversation);
                    if (!ret && conversation.Exception != null && conversation.Exception is OmniCoreProtocolException)
                    {
                        retries++;
                        if (requestMessage.RequestType != RequestType.Status)
                        {
                            var statusRequest = new ErosMessageBuilder().WithStatus(0).Build();
                            await PerformExchangeInternal(statusRequest, messageExchangeParameters, conversation);
                        }
                    }
                    else
                        return ret;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return false;
        }

        private async Task<bool> PerformExchangeInternal(IMessage requestMessage, IMessageExchangeParameters messageExchangeParameters,
                    IConversation conversation)
        {
            var emp = messageExchangeParameters as ErosMessageExchangeParameters;
            var progress = conversation.NewExchange(requestMessage);
            try
            {
                progress.ActionText = "Started new message exchange";
                progress.Result.RequestTime = DateTimeOffset.UtcNow;
                progress.Running = true;
                var messageExchange = await MessageExchangeProvider.GetMessageExchange(messageExchangeParameters, Pod);
                await messageExchange.InitializeExchange(progress);
                var response = await messageExchange.GetResponse(requestMessage, progress);

                messageExchange.ParseResponse(response, Pod, progress);

                if (ErosPod.RuntimeVariables.NonceSync.HasValue)
                {
                    var responseMessage = response as ErosMessage;
                    emp.MessageSequenceOverride = (responseMessage.sequence + 15) % 16;
                    messageExchange = await MessageExchangeProvider.GetMessageExchange(messageExchangeParameters, Pod);
                    await messageExchange.InitializeExchange(progress);
                    response = await messageExchange.GetResponse(requestMessage, progress);
                    messageExchange.ParseResponse(response, Pod, progress);
                    if (ErosPod.RuntimeVariables.NonceSync.HasValue)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Nonce re-negotiation failed");
                }
                progress.Result.Success = true;
            }
            catch (Exception e)
            {
                progress.SetException(e);
            }
            finally
            {
                progress.Result.ResultTime = DateTimeOffset.UtcNow;
                progress.Running = false;
                progress.Finished = true;
                ErosRepository.Instance.Save(ErosPod, progress.Result);
            }

            return progress.Result.Success;
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
                if (!await this.UpdateStatusInternal(conversation, updateType))
                    return;
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
                if (Pod.LastStatus.Progress < PodProgress.PairingSuccess)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod not paired completely yet.");

                if (Pod.LastStatus.Progress == PodProgress.ErrorShuttingDown)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is shutting down, cannot acknowledge alerts.");

                if (Pod.LastStatus.Progress == PodProgress.AlertExpiredShuttingDown)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Acknowledgement period expired, pod is shutting down");

                if (Pod.LastStatus.Progress > PodProgress.AlertExpiredShuttingDown)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is not active");

                if ((Pod.LastStatus.AlertMask & alertMask) != alertMask)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Bitmask is invalid for current alert state");

                var request = new ErosMessageBuilder().WithAcknowledgeAlerts(alertMask).Build();
                if (!await PerformExchange(request, GetStandardParameters(), conversation))
                    return;

                if ((Pod.LastStatus.AlertMask & alertMask) != 0)
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

                if (Pod.LastStatus.BasalState == BasalState.Temporary)
                {
                    var cancelReq = new ErosMessageBuilder().WithCancelTempBasal().Build();
                    if (!await PerformExchange(cancelReq, GetStandardParameters(), conversation))
                        return;
                }

                if (Pod.LastStatus.BasalState == BasalState.Temporary)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod is still executing a temp basal");

                var request = new ErosMessageBuilder().WithTempBasal(basalRate, durationInHours).Build();
                if (!await PerformExchange(request, GetStandardParameters(), conversation))
                    return;

                if (Pod.LastStatus.BasalState != BasalState.Temporary)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not start the temp basal");

                Pod.LastTempBasalResult = conversation.CurrentExchange.Result;
            }
            catch (Exception e)
            {
                conversation.Exception = e;
            }
        }

        public async Task Bolus(IConversation conversation, decimal bolusAmount, bool waitForBolusToFinish = true)
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

                if (Pod.LastStatus.BolusState != BolusState.Immediate)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not start bolusing");

                if (waitForBolusToFinish)
                {
                    while (Pod.LastStatus.BolusState == BolusState.Immediate)
                    {
                        var tickCount = (int)(Pod.LastStatus.NotDeliveredInsulin / 0.05m);
                        await Task.Delay(tickCount * 2000 + 500, conversation.Token);

                        if (conversation.Token.IsCancellationRequested)
                        {
                            var cancelRequest = new ErosMessageBuilder().WithCancelBolus().Build();
                            var cancelResult = await PerformExchange(request, GetStandardParameters(), conversation);

                            if (!cancelResult || Pod.LastStatus.BolusState == BolusState.Immediate)
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

                    if (Pod.LastStatus.NotDeliveredInsulin != 0)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Not all insulin was delivered");
                }
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

                if (Pod.LastStatus.Progress >= PodProgress.Inactive)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod already deactivated");

                var request = new ErosMessageBuilder().WithDeactivate().Build();
                if (!await PerformExchange(request, GetStandardParameters(), conversation))
                    return;

                if (Pod.LastStatus.Progress != PodProgress.Inactive)
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

                if (Pod.LastStatus == null || Pod.LastStatus.Progress <= PodProgress.TankFillCompleted)
                {
                    var parameters = GetStandardParameters();
                    parameters.AddressOverride = 0xffffffff;
                    parameters.AckAddressOverride = Pod.RadioAddress;
                    parameters.TransmissionLevelOverride = TxPower.A3_BelowNormal;
                    parameters.AllowAutoLevelAdjustment = false;

                    var request = new ErosMessageBuilder().WithAssignAddress(Pod.RadioAddress).Build();
                    if (!await PerformExchange(request, parameters, conversation))
                        return;

                    if (Pod.LastStatus == null)
                        throw new OmniCoreWorkflowException(FailureType.RadioRecvTimeout, "Pod did not respond to pairing request");
                    else if (Pod.LastStatus.Progress < PodProgress.TankFillCompleted)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod is not filled with enough insulin for activation.");

                }

                if (Pod.LastStatus != null && Pod.LastStatus.Progress < PodProgress.PairingSuccess)
                {
                    Pod.ActivationDate = DateTimeOffset.UtcNow;
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

                if (Pod.LastStatus.Progress > PodProgress.ReadyForInjection)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is already activated");

                if (Pod.LastStatus.Progress == PodProgress.PairingSuccess)
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

                    if (Pod.LastStatus.Progress != PodProgress.Purging)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not start priming");

                }

                while (Pod.LastStatus.Progress == PodProgress.Purging)
                {
                    var ticks = (int)(Pod.LastStatus.NotDeliveredInsulin / 0.05m);
                    await Task.Delay(ticks * 1000 + 200);

                    if (!await UpdateStatusInternal(conversation))
                        return;
                }

                if (Pod.LastStatus.Progress != PodProgress.ReadyForInjection)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not reach ready for injection state.");

                if (Pod.LastUserSettings?.ExpiryWarningAtMinute != null)
                {
                    //TODO: expiry warning
                }
            }
            catch (Exception e)
            {
                conversation.Exception = e;
            }
        }

        public async Task InjectAndStart(IConversation conversation, IProfile profile)
        {
            try
            {
                // progress.CommandText = $"Starting Pod";
                if (!await UpdateStatusInternal(conversation))
                    return;

                if (Pod.LastStatus.Progress >= PodProgress.Running)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is already started");

                if (Pod.LastStatus.Progress < PodProgress.ReadyForInjection)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is not ready for injection");

                if (Pod.LastStatus.Progress == PodProgress.ReadyForInjection)
                {
                    AssertBasalScheduleValid(profile.BasalSchedule);

                    var podDate = DateTimeOffset.UtcNow + TimeSpan.FromMinutes(profile.UtcOffset);
                    var parameters = GetStandardParameters();
                    parameters.RepeatFirstPacket = true;
                    parameters.CriticalWithFollowupRequired = true;

                    var request = new ErosMessageBuilder()
                        .WithBasalSchedule(profile.BasalSchedule, (ushort)podDate.Hour, (ushort)podDate.Minute, (ushort)podDate.Second)
                        .Build();

                    if (!await PerformExchange(request, parameters, conversation))
                        return;

                    if (Pod.LastStatus.Progress != PodProgress.BasalScheduleSet)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not acknowledge basal schedule");
                }

                if (Pod.LastStatus.Progress == PodProgress.BasalScheduleSet)
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

                    if (Pod.LastStatus.Progress != PodProgress.Priming)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not start priming the cannula for insertion");

                    Pod.InsertionDate = DateTimeOffset.UtcNow;
                }

                while (Pod.LastStatus.Progress == PodProgress.Priming)
                {
                    var ticks = (int)(Pod.LastStatus.NotDeliveredInsulin / 0.05m);
                    await Task.Delay(ticks * 1000 + 200);

                    if (!await UpdateStatusInternal(conversation))
                        return;
                }

                if (Pod.LastStatus.Progress != PodProgress.Running)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not enter the running state");

                Pod.ReservoirUsedForPriming = Pod.LastStatus.DeliveredInsulin;

            }
            catch (Exception e)
            {
                conversation.Exception = e;
            }
        }

        public async Task ConfigureAlerts(IConversation conversation, AlertConfiguration[] alertConfigurations)
        {
            throw new NotImplementedException();
        }

        public async Task CancelBolus(IConversation conversation)
        {
            try
            {
                AssertRunningStatus();
                AssertImmediateBolusActive();

                var request = new ErosMessageBuilder().WithCancelBolus().Build();
                if (!await PerformExchange(request, GetStandardParameters(), conversation))
                    return;

                if (Pod.LastStatus.BolusState != BolusState.Inactive)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not cancel the bolus");
            }
            catch (Exception e)
            {
                conversation.Exception = e;
            }
        }

        public async Task CancelTempBasal(IConversation conversation)
        {
            try
            {
                if (!await UpdateStatusInternal(conversation))
                    return;

                AssertRunningStatus();
                AssertImmediateBolusInactive();

                var request = new ErosMessageBuilder().WithCancelTempBasal().Build();
                if (Pod.LastStatus.BasalState == BasalState.Temporary)
                {
                    if (!await PerformExchange(request, GetStandardParameters(), conversation))
                        return;
                }
                else
                {
                    var emp = GetStandardParameters();
                    var progress = conversation.NewExchange(request);
                    progress.Result.ResultTime = DateTimeOffset.UtcNow;
                    progress.Result.Success = true;
                    progress.Result.Status = Pod.LastStatus;
                    progress.Running = false;
                    progress.Finished = true;
                    
                    ErosRepository.Instance.Save(ErosPod, progress.Result);
                }

                if (Pod.LastStatus.BasalState != BasalState.Scheduled)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not cancel the temp basal");

                Pod.LastTempBasalResult = null;
            }
            catch (Exception e)
            {
                conversation.Exception = e;
            }
        }

        public Task StartExtendedBolus(IConversation conversation, decimal bolusAmount, decimal durationInHours)
        {
            throw new NotImplementedException();
        }

        public Task CancelExtendedBolus(IConversation conversation)
        {
            throw new NotImplementedException();
        }

        public async Task SetBasalSchedule(IConversation conversation, IProfile profile)
        {
            try
            {
                if (!await UpdateStatusInternal(conversation))
                    return;

                AssertRunningStatus();
                AssertImmediateBolusInactive();

                if (Pod.LastStatus.BasalState == BasalState.Temporary)
                {
                    var cancelReq = new ErosMessageBuilder().WithCancelTempBasal().Build();
                    if (!await PerformExchange(cancelReq, GetStandardParameters(), conversation))
                        return;
                }

                if (Pod.LastStatus.BasalState == BasalState.Temporary)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not cancel the temp basal");

                AssertBasalScheduleValid(profile.BasalSchedule);

                var podDate = DateTimeOffset.UtcNow + TimeSpan.FromMinutes(profile.UtcOffset);
                var parameters = GetStandardParameters();
                //parameters.RepeatFirstPacket = true;
                parameters.CriticalWithFollowupRequired = false;

                var request = new ErosMessageBuilder()
                    .WithBasalSchedule(profile.BasalSchedule, (ushort)podDate.Hour, (ushort)podDate.Minute, (ushort)podDate.Second)
                    .Build();

                var progress = conversation.NewExchange(request);
                progress.Result.BasalSchedule = new ErosBasalSchedule()
                {
                    BasalSchedule = profile.BasalSchedule,
                    PodDateTime = podDate,
                    UtcOffset = profile.UtcOffset
                };

                await PerformExchange(request, parameters, conversation);

            }
            catch (Exception e)
            {
                conversation.Exception = e;
            }
        }

        public Task SuspendBasal(IConversation conversation)
        {
            throw new NotImplementedException();
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
            if (Pod.LastStatus != null && Pod.LastStatus.BolusState == BolusState.Immediate)
                throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Bolus operation in progress");
        }

        private void AssertImmediateBolusActive()
        {
            if (Pod.LastStatus != null && Pod.LastStatus.BolusState != BolusState.Immediate)
                throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "No bolus operation in progress");
        }

        private void AssertNotPaired()
        {
            if (Pod.LastStatus != null && Pod.LastStatus.Progress >= PodProgress.PairingSuccess)
                throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is already paired");
        }

        private void AssertPaired()
        {
            if (Pod.LastStatus == null || Pod.LastStatus.Progress < PodProgress.PairingSuccess)
                throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is not paired");
        }

        private void AssertRunningStatus()
        {
            if (Pod.LastStatus == null || Pod.LastStatus.Progress < PodProgress.Running)
                throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is not yet running");

            if (Pod.LastStatus == null || Pod.LastStatus.Progress > PodProgress.RunningLow)
                throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is not running");
        }
    }
}
