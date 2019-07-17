using Microsoft.AppCenter.Crashes;
using OmniCore.Mobile.Base;
using OmniCore.Mobile.Base.Interfaces;
using OmniCore.Model.Enums;
using OmniCore.Model.Eros.Data;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Model.Eros
{
    public partial class ErosPod : IPod
    {
        readonly IMessageExchangeProvider MessageExchangeProvider;

        private Nonce nonce;
        public Nonce Nonce
        {
            get
            {
                if (nonce == null)
                {
                    if (Lot != null && Serial != null)
                        nonce = new Nonce(this);
                }
                return nonce;
            }
        }

        public ErosPod(IMessageExchangeProvider messageExchangeProvider)
        {
            MessageExchangeProvider = messageExchangeProvider;
        }

        public async Task<IConversation> StartConversation(IMessageExchangeProvider messageExchangeProvider,
            string intent,
            int timeout = 0,
            RequestSource source = RequestSource.OmniCoreUser)
        {
            int started = Environment.TickCount;
            while (!OmniCoreServices.AppState.TrySet(AppStateConstants.ActiveConversation, intent))
            {
                if (timeout > 0 && Environment.TickCount - started > timeout)
                    throw new OmniCoreTimeoutException(FailureType.OperationInProgress, "Timed out waiting for existing operation to complete");
                await Task.Delay(250);
            }

            IConversation conversation = null;
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

                conversation = new ErosConversation(wakeLock, messageExchangeProvider, this) { RequestSource = source, Intent = intent };
                MessagingCenter.Send<IConversation>(conversation, MessagingConstants.ConversationStarted);
                return conversation;
            }
            catch(Exception e)
            {
                Crashes.TrackError(e);
                wakeLock?.Dispose();
                OmniCoreServices.AppState.TryRemove(AppStateConstants.ActiveConversation);
                conversation?.Dispose();
                throw;
            }
        }

        private ErosMessageExchangeParameters GetStandardParameters()
        {
            return new ErosMessageExchangeParameters() { Nonce = Nonce, AllowAutoLevelAdjustment = true };
        }

        private async Task<bool> PerformExchange(IMessage requestMessage, IMessageExchangeParameters messageExchangeParameters,
            IConversation conversation, Action<IMessageExchangeResult> resultModifier = null)
        {
            int retries = 0;
            IMessageExchange exchange = null;
            while(retries < 2)
            {
                try
                {
                    exchange = await PerformExchangeInternal(requestMessage, messageExchangeParameters, conversation, resultModifier);
                    if (!exchange.Result.Success && conversation.Exception != null && conversation.Exception is OmniCoreProtocolException)
                    {
                        retries++;
                        if (requestMessage.RequestType != RequestType.Status)
                        {
                            var statusRequest = new ErosMessageBuilder().WithStatus(0).Build();
                            exchange = await PerformExchangeInternal(statusRequest, messageExchangeParameters, conversation);
                        }
                    }
                    else
                        break;
                }
                catch (Exception e)
                {
                    Crashes.TrackError(e);
                    throw;
                }
            }
            return exchange.Result.Success;
        }

        private async Task<IMessageExchange> PerformExchangeInternal(IMessage requestMessage, IMessageExchangeParameters messageExchangeParameters,
                    IConversation conversation, Action<IMessageExchangeResult> resultModifier = null)
        {
            var emp = messageExchangeParameters as ErosMessageExchangeParameters;
            var exchange = await conversation.NewExchange(messageExchangeParameters);
            try
            {
                exchange.ActionText = "Started new message exchange";
                exchange.Result.RequestTime = DateTimeOffset.UtcNow;
                exchange.Running = true;
                var response = await exchange.GetResponse(requestMessage);

                if (RuntimeVariables.NonceSync.HasValue)
                {
                    var responseMessage = response as ErosMessage;
                    emp.MessageSequenceOverride = (responseMessage.sequence + 15) % 16;
                    exchange = await conversation.NewExchange(messageExchangeParameters);
                    response = await exchange.GetResponse(requestMessage);
                    if (RuntimeVariables.NonceSync.HasValue)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Nonce re-negotiation failed");
                }
                exchange.Result.Success = true;
            }
            catch (Exception e)
            {
                exchange.SetException(e);
            }
            finally
            {
                if (resultModifier != null)
                {
                    try
                    {
                        await Task.Run(() => resultModifier(exchange.Result));
                    }
                    catch(AggregateException ae)
                    {
                        exchange.SetException(ae);
                    }
                    catch(Exception e)
                    {
                        exchange.SetException(e);
                    }
                }
                exchange.Result.ResultTime = DateTimeOffset.UtcNow;
                exchange.Running = false;
                exchange.Finished = true;
                var repo = await ErosRepository.GetInstance();
                await repo.Save(this, exchange);
            }

            return exchange;
        }

        private async Task<bool> UpdateStatusInternal(IConversation conversation,
            StatusRequestType updateType = StatusRequestType.Standard,
            int? timeout = null)
        {
            var request = new ErosMessageBuilder().WithStatus(updateType).Build();
            var parameters = GetStandardParameters();
            parameters.FirstExchangeTimeout = timeout;
            return await PerformExchange(request, parameters, conversation);
        }

        public async Task UpdateStatus(IConversation conversation, 
            StatusRequestType updateType = StatusRequestType.Standard,
            int? timeout = null)
        {
            try
            {
                if (!await this.UpdateStatusInternal(conversation, updateType, timeout))
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
                if (LastStatus.Progress < PodProgress.PairingSuccess)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod not paired completely yet.");

                if (LastStatus.Progress == PodProgress.ErrorShuttingDown)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is shutting down, cannot acknowledge alerts.");

                if (LastStatus.Progress == PodProgress.AlertExpiredShuttingDown)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Acknowledgement period expired, pod is shutting down");

                if (LastStatus.Progress > PodProgress.AlertExpiredShuttingDown)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is not active");

                if ((LastStatus.AlertMask & alertMask) != alertMask)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Bitmask is invalid for current alert state");

                var request = new ErosMessageBuilder().WithAcknowledgeAlerts(alertMask).Build();
                if (!await PerformExchange(request, GetStandardParameters(), conversation))
                    return;

                if ((LastStatus.AlertMask & alertMask) != 0)
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

                if (LastStatus.BasalState == BasalState.Temporary)
                {
                    var cancelReq = new ErosMessageBuilder().WithCancelTempBasal().Build();
                    if (!await PerformExchange(cancelReq, GetStandardParameters(), conversation))
                        return;
                }

                if (LastStatus.BasalState == BasalState.Temporary)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod is still executing a temp basal");

                var request = new ErosMessageBuilder().WithTempBasal(basalRate, durationInHours).Build();
                if (!await PerformExchange(request, GetStandardParameters(), conversation))
                    return;

                if (LastStatus.BasalState != BasalState.Temporary)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not start the temp basal");

                LastTempBasalResult = conversation.CurrentExchange.Result;
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

                if (LastStatus.BolusState != BolusState.Immediate)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not start bolusing");

                if (waitForBolusToFinish)
                {
                    while (LastStatus.BolusState == BolusState.Immediate)
                    {
                        var tickCount = (int)(LastStatus.NotDeliveredInsulin / 0.05m);
                        await Task.Delay(tickCount * 2000 + 500, conversation.Token);

                        if (conversation.Token.IsCancellationRequested)
                        {
                            var cancelRequest = new ErosMessageBuilder().WithCancelBolus().Build();
                            var cancelResult = await PerformExchange(request, GetStandardParameters(), conversation);

                            if (!cancelResult || LastStatus.BolusState == BolusState.Immediate)
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

                    if (LastStatus.NotDeliveredInsulin != 0)
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

                if (LastStatus.Progress >= PodProgress.Inactive)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod already deactivated");

                var request = new ErosMessageBuilder().WithDeactivate().Build();
                if (!await PerformExchange(request, GetStandardParameters(), conversation))
                    return;

                if (LastStatus.Progress != PodProgress.Inactive)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Failed to deactivate");
            }
            catch (Exception e)
            {
                conversation.Exception = e;
            }
        }

        public async Task Pair(IConversation conversation, IProfile profile)
        {
            try
            {
                // progress.CommandText = $"Pairing with Pod";

                AssertNotPaired();

                if (LastStatus == null || LastStatus.Progress <= PodProgress.TankFillCompleted)
                {
                    var parameters = GetStandardParameters();
                    parameters.AddressOverride = 0xffffffff;
                    parameters.AckAddressOverride = RadioAddress;
                    parameters.TransmissionLevelOverride = TxPower.A3_BelowNormal;
                    parameters.AllowAutoLevelAdjustment = false;

                    var request = new ErosMessageBuilder().WithAssignAddress(RadioAddress).Build();
                    if (!await PerformExchange(request, parameters, conversation))
                        return;

                    if (LastStatus == null)
                        throw new OmniCoreWorkflowException(FailureType.RadioRecvTimeout, "Pod did not respond to pairing request");
                    else if (LastStatus.Progress < PodProgress.TankFillCompleted)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod is not filled with enough insulin for activation.");

                }

                if (LastStatus != null && LastStatus.Progress < PodProgress.PairingSuccess)
                {
                    ActivationDate = DateTimeOffset.UtcNow;
                    var podDate = ActivationDate.Value + TimeSpan.FromMinutes(profile.UtcOffset);
                    var parameters = GetStandardParameters();
                    parameters.AddressOverride = 0xffffffff;
                    parameters.AckAddressOverride = RadioAddress;
                    parameters.TransmissionLevelOverride = TxPower.A3_BelowNormal;
                    parameters.MessageSequenceOverride = 1;
                    parameters.AllowAutoLevelAdjustment = false;

                    var request = new ErosMessageBuilder().WithSetupPod(Lot.Value, Serial.Value, RadioAddress,
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

                if (LastStatus.Progress > PodProgress.ReadyForInjection)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is already activated");

                if (LastStatus.Progress == PodProgress.PairingSuccess)
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

                    if (LastStatus.Progress != PodProgress.Purging)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not start priming");

                }

                while (LastStatus.Progress == PodProgress.Purging)
                {
                    var ticks = (int)(LastStatus.NotDeliveredInsulin / 0.05m);
                    await Task.Delay(ticks * 1000 + 200);

                    if (!await UpdateStatusInternal(conversation))
                        return;
                }

                if (LastStatus.Progress != PodProgress.ReadyForInjection)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not reach ready for injection state.");

                if (LastUserSettings?.ExpiryWarningAtMinute != null)
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

                if (LastStatus.Progress >= PodProgress.Running)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is already started");

                if (LastStatus.Progress < PodProgress.ReadyForInjection)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is not ready for injection");

                if (LastStatus.Progress == PodProgress.ReadyForInjection)
                {
                    var parameters = GetStandardParameters();
                    parameters.RepeatFirstPacket = true;
                    parameters.CriticalWithFollowupRequired = true;
                    await SetBasalScheduleInternal(conversation, profile, parameters);

                    if (LastStatus.Progress != PodProgress.BasalScheduleSet)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not acknowledge basal schedule");
                }

                if (LastStatus.Progress == PodProgress.BasalScheduleSet)
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

                    if (LastStatus.Progress != PodProgress.Priming)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not start priming the cannula for insertion");

                    InsertionDate = DateTimeOffset.UtcNow;
                }

                while (LastStatus.Progress == PodProgress.Priming)
                {
                    var ticks = (int)(LastStatus.NotDeliveredInsulin / 0.05m);
                    await Task.Delay(ticks * 1000 + 200);

                    if (!await UpdateStatusInternal(conversation))
                        return;
                }

                if (LastStatus.Progress != PodProgress.Running)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not enter the running state");

                ReservoirUsedForPriming = LastStatus.DeliveredInsulin;

            }
            catch (Exception e)
            {
                conversation.Exception = e;
            }
        }

        [method: SuppressMessage("", "CS1998", Justification = "Not implemented")]
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

                if (LastStatus.BolusState != BolusState.Inactive)
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

                if (LastStatus.BasalState == BasalState.Temporary)
                {
                    var request = new ErosMessageBuilder().WithCancelTempBasal().Build();
                    if (!await PerformExchange(request, GetStandardParameters(), conversation))
                        return;

                    if (LastStatus.BasalState != BasalState.Scheduled)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not cancel the temp basal");
                }

                LastTempBasalResult = null;
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

                if (LastStatus.BasalState == BasalState.Temporary)
                {
                    var cancelReq = new ErosMessageBuilder().WithCancelTempBasal().Build();
                    if (!await PerformExchange(cancelReq, GetStandardParameters(), conversation))
                        return;
                }

                if (LastStatus.BasalState == BasalState.Temporary)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not cancel the temp basal");

                var parameters = GetStandardParameters();
                parameters.RepeatFirstPacket = false;
                parameters.CriticalWithFollowupRequired = false;

                await SetBasalScheduleInternal(conversation, profile, parameters);
            }
            catch (Exception e)
            {
                conversation.Exception = e;
            }
        }

        private async Task SetBasalScheduleInternal(IConversation conversation, IProfile profile, IMessageExchangeParameters parameters)
        {
            try
            {
                AssertBasalScheduleValid(profile.BasalSchedule);

                var podDate = DateTimeOffset.UtcNow + TimeSpan.FromMinutes(profile.UtcOffset);

                var request = new ErosMessageBuilder()
                    .WithBasalSchedule(profile.BasalSchedule, (ushort)podDate.Hour, (ushort)podDate.Minute, (ushort)podDate.Second)
                    .Build();

                await PerformExchange(request, parameters, conversation, (result) =>
                {
                    if (result.Success)
                    {
                        result.BasalSchedule = new ErosBasalSchedule()
                        {
                            BasalSchedule = profile.BasalSchedule,
                            PodDateTime = podDate,
                            UtcOffset = profile.UtcOffset
                        };
                    }
                });
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
            if (LastStatus != null && LastStatus.BolusState == BolusState.Immediate)
                throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Bolus operation in progress");
        }

        private void AssertImmediateBolusActive()
        {
            if (LastStatus != null && LastStatus.BolusState != BolusState.Immediate)
                throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "No bolus operation in progress");
        }

        private void AssertNotPaired()
        {
            if (LastStatus != null && LastStatus.Progress >= PodProgress.PairingSuccess)
                throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is already paired");
        }

        private void AssertPaired()
        {
            if (LastStatus == null || LastStatus.Progress < PodProgress.PairingSuccess)
                throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is not paired");
        }

        private void AssertRunningStatus()
        {
            if (LastStatus == null || LastStatus.Progress < PodProgress.Running)
                throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is not yet running");

            if (LastStatus == null || LastStatus.Progress > PodProgress.RunningLow)
                throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is not running");
        }
    }
}
