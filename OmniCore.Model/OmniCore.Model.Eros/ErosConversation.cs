using Microsoft.AppCenter.Crashes;
using OmniCore.Mobile.Base;
using OmniCore.Mobile.Base.Interfaces;
using OmniCore.Model.Enums;
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
    public class ErosConversation : IConversation
    {
        public DateTimeOffset Started { get; }
        public DateTimeOffset? Ended { get; }
        public string Intent { get; set; }
        public ExchangeStatus Status { get; }
        public bool CanCancel { get; set; }
        public bool IsRunning { get; set; }
        public bool IsFinished { get; set; }
        public bool Failed { get; set; }
        public bool Canceled { get; set; }
        public FailureType FailureType { get; set; }
        public RequestSource Requestor { get; set; }

        public Exception Exception
        {
            get => exception;
            set
            {
                CanCancel = false;
                IsRunning = false;
                IsFinished = true;
                Failed = true;
                var oe = value as OmniCoreException;
                FailureType = oe?.FailureType ?? FailureType.Unknown;
                exception = value;
            }
        }

        public IMessageExchange CurrentExchange { get; set; }
        public IPod Pod { get; private set; }

        public RequestSource RequestedBy { get; }
        public CancellationToken Token => CancellationTokenSource.Token;

        public IRadio Radio { get; private set; }

        private Exception exception;
        private readonly IWakeLock WakeLock;
        private readonly CancellationTokenSource CancellationTokenSource;
        private TaskCompletionSource<bool> CancellationCompletion;

        public ErosConversation(IWakeLock wakeLock, IRadio radio, IPod pod)
        {
            Pod = pod;
            Radio = radio;
            WakeLock = wakeLock;
            Started = DateTimeOffset.UtcNow;
            CancellationTokenSource = new CancellationTokenSource();
        }

        public async Task<bool> Cancel()
        {
            if (!CanCancel)
                return false;

            if (!CancellationTokenSource.IsCancellationRequested)
            {
                CancellationCompletion = new TaskCompletionSource<bool>();
                CancellationTokenSource.Cancel();
            }

            var result = await CancellationCompletion.Task;
            if (result)
            {
                IsRunning = false;
                IsFinished = true;
                Canceled = true;
            }
            return result;
        }

        public void CancelComplete()
        {
            if (CancellationTokenSource.IsCancellationRequested)
            {
                CanCancel = false;
                CancellationCompletion.TrySetResult(true);
            }
        }

        public void CancelFailed()
        {
            if (CancellationTokenSource.IsCancellationRequested)
            {
                CanCancel = false;
                CancellationCompletion.TrySetResult(false);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    IsFinished = true;
                    Ended = DateTimeOffset.UtcNow;
                    WakeLock.Release();
                    OmniCoreServices.AppState.TryRemove(AppStateConstants.ActiveConversation);
                    CancellationTokenSource.Dispose();
                    MessagingCenter.Send<IConversation>(this, MessagingConstants.ConversationEnded);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ErosConversation()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        readonly IMessageExchangeProvider MessageExchangeProvider;

        private Nonce nonce;
        public Nonce Nonce
        {
            get
            {
                if (nonce == null)
                {
                    if (Pod.Lot != null && Pod.Serial != null)
                        nonce = new Nonce(Pod as ErosPod);
                }
                return nonce;
            }
        }

        public ErosConversation(IMessageExchangeProvider messageExchangeProvider)
        {
            MessageExchangeProvider = messageExchangeProvider;
        }



        private ErosMessageExchangeParameters GetStandardParameters()
        {
            return new ErosMessageExchangeParameters() { Nonce = Nonce, AllowAutoLevelAdjustment = true };
        }

        private async Task<bool> PerformExchange(IMessage requestMessage, IMessageExchangeParameters messageExchangeParameters,
            Action<IMessageExchangeResult> resultModifier = null)
        {
            int retries = 0;
            IMessageExchange exchange = null;
            while(retries < 2)
            {
                try
                {
                    exchange = await PerformExchangeInternal(requestMessage, messageExchangeParameters, resultModifier);
                    if (!exchange.Result.Success && this.Exception != null && this.Exception is OmniCoreProtocolException)
                    {
                        retries++;
                        if (requestMessage.RequestType != RequestType.Status)
                        {
                            var statusRequest = new ErosMessageBuilder().WithStatus(0).Build();
                            exchange = await PerformExchangeInternal(statusRequest, messageExchangeParameters);
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
                    Action<IMessageExchangeResult> resultModifier = null)
        {
            var emp = messageExchangeParameters as ErosMessageExchangeParameters;
            var exchange = await NewExchange(messageExchangeParameters);
            try
            {
                exchange.ActionText = "Started new message exchange";
                exchange.Result.RequestTime = DateTimeOffset.UtcNow;
                exchange.Running = true;
                var response = await exchange.GetResponse(requestMessage);

                if ((Pod as ErosPod).RuntimeVariables.NonceSync.HasValue)
                {
                    var responseMessage = response as ErosMessage;
                    emp.MessageSequenceOverride = (responseMessage.sequence + 15) % 16;
                    exchange = await NewExchange(messageExchangeParameters);
                    response = await exchange.GetResponse(requestMessage);
                    if ((Pod as ErosPod).RuntimeVariables.NonceSync.HasValue)
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
                await repo.Save(Pod, exchange);
            }

            return exchange;
        }

        private async Task<bool> UpdateStatusInternal(
            StatusRequestType updateType = StatusRequestType.Standard,
            int? timeout = null)
        {
            var request = new ErosMessageBuilder().WithStatus(updateType).Build();
            var parameters = GetStandardParameters();
            parameters.FirstExchangeTimeout = timeout;
            return await PerformExchange(request, parameters);
        }

        public async Task UpdateStatus(
            StatusRequestType updateType = StatusRequestType.Standard,
            int? timeout = null)
        {
            try
            {
                if (!await this.UpdateStatusInternal(updateType, timeout))
                    return;
            }
            catch (Exception e)
            {
                this.Exception = e;
            }
        }

        public async Task AcknowledgeAlerts(byte alertMask)
        {
            try
            {
                Debug.WriteLine($"Acknowledging alerts, bitmask: {alertMask}");
                if (!await UpdateStatusInternal())
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
                if (!await PerformExchange(request, GetStandardParameters()))
                    return;

                if ((Pod.LastStatus.AlertMask & alertMask) != 0)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Alerts not completely acknowledged");

            }
            catch (Exception e)
            {
                this.Exception = e;
            }
        }

        public async Task SetTempBasal(decimal basalRate, decimal durationInHours)
        {
            try
            {
                // progress.CommandText = $"Set Temp Basal {basalRate}U/h for {durationInHours}h";
                if (!await UpdateStatusInternal())
                    return;

                AssertRunningStatus();
                AssertImmediateBolusInactive();

                if (Pod.LastStatus.BasalState == BasalState.Temporary)
                {
                    var cancelReq = new ErosMessageBuilder().WithCancelTempBasal().Build();
                    if (!await PerformExchange(cancelReq, GetStandardParameters()))
                        return;
                }

                if (Pod.LastStatus.BasalState == BasalState.Temporary)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod is still executing a temp basal");

                var request = new ErosMessageBuilder().WithTempBasal(basalRate, durationInHours).Build();
                if (!await PerformExchange(request, GetStandardParameters()))
                    return;

                if (Pod.LastStatus.BasalState != BasalState.Temporary)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not start the temp basal");

                Pod.LastTempBasalResult = CurrentExchange.Result;
            }
            catch (Exception e)
            {
                this.Exception = e;
            }
        }

        public async Task Bolus(decimal bolusAmount, bool waitForBolusToFinish = true)
        {
            try
            {
                //progress.CommandText = $"Bolusing {bolusAmount}U";
                Debug.WriteLine($"Bolusing {bolusAmount}U");
                if (!await UpdateStatusInternal())
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
                if (!await PerformExchange(request, GetStandardParameters()))
                    return;

                if (Pod.LastStatus.BolusState != BolusState.Immediate)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not start bolusing");

                if (waitForBolusToFinish)
                {
                    while (Pod.LastStatus.BolusState == BolusState.Immediate)
                    {
                        var tickCount = (int)(Pod.LastStatus.NotDeliveredInsulin / 0.05m);
                        await Task.Delay(tickCount * 2000 + 500, Token);

                        if (Token.IsCancellationRequested)
                        {
                            var cancelRequest = new ErosMessageBuilder().WithCancelBolus().Build();
                            var cancelResult = await PerformExchange(request, GetStandardParameters());

                            if (!cancelResult || Pod.LastStatus.BolusState == BolusState.Immediate)
                            {
                                CancelFailed();
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (!await UpdateStatusInternal())
                            return;
                    }

                    if (Canceled || Failed)
                        return;

                    if (Pod.LastStatus.NotDeliveredInsulin != 0)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Not all insulin was delivered");
                }
            }
            catch (Exception e)
            {
                this.Exception = e;
            }
        }

        public async Task Deactivate()
        {
            try
            {
                // progress.CommandText = $"Deactivating Pod";
                AssertPaired();

                if (Pod.LastStatus.Progress >= PodProgress.Inactive)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod already deactivated");

                var request = new ErosMessageBuilder().WithDeactivate().Build();
                if (!await PerformExchange(request, GetStandardParameters()))
                    return;

                if (Pod.LastStatus.Progress != PodProgress.Inactive)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Failed to deactivate");
            }
            catch (Exception e)
            {
                this.Exception = e;
            }
        }

        public async Task Pair(IProfile profile)
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
                    if (!await PerformExchange(request, parameters))
                        return;

                    if (Pod.LastStatus == null)
                        throw new OmniCoreWorkflowException(FailureType.RadioRecvTimeout, "Pod did not respond to pairing request");
                    else if (Pod.LastStatus.Progress < PodProgress.TankFillCompleted)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod is not filled with enough insulin for activation.");

                }

                if (Pod.LastStatus != null && Pod.LastStatus.Progress < PodProgress.PairingSuccess)
                {
                    Pod.ActivationDate = DateTimeOffset.UtcNow;
                    var podDate = Pod.ActivationDate.Value + TimeSpan.FromMinutes(profile.UtcOffset);
                    var parameters = GetStandardParameters();
                    parameters.AddressOverride = 0xffffffff;
                    parameters.AckAddressOverride = Pod.RadioAddress;
                    parameters.TransmissionLevelOverride = TxPower.A3_BelowNormal;
                    parameters.MessageSequenceOverride = 1;
                    parameters.AllowAutoLevelAdjustment = false;

                    var request = new ErosMessageBuilder().WithSetupPod(Pod.Lot.Value, Pod.Serial.Value, Pod.RadioAddress,
                        podDate.Year, (byte)podDate.Month, (byte)podDate.Day,
                        (byte)podDate.Hour, (byte)podDate.Minute).Build();

                    if (!await PerformExchange(request, parameters))
                        return;

                    AssertPaired();
                }
            }
            catch (Exception e)
            {
                this.Exception = e;
            }
        }

        public async Task Activate()
        {
            try
            {
                // progress.CommandText = $"Activating Pod";
                if (!await UpdateStatusInternal())
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

                    if (!await PerformExchange(request, parameters))
                        return;

                    request = new ErosMessageBuilder().WithDeliveryFlags(0, 0).Build();
                    if (!await PerformExchange(request, parameters))
                        return;

                    request = new ErosMessageBuilder().WithPrimeCannula().Build();
                    if (!await PerformExchange(request, parameters))
                        return;

                    if (Pod.LastStatus.Progress != PodProgress.Purging)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not start priming");

                }

                while (Pod.LastStatus.Progress == PodProgress.Purging)
                {
                    var ticks = (int)(Pod.LastStatus.NotDeliveredInsulin / 0.05m);
                    await Task.Delay(ticks * 1000 + 200);

                    if (!await UpdateStatusInternal())
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
                this.Exception = e;
            }
        }

        public async Task InjectAndStart(IProfile profile)
        {
            try
            {
                // progress.CommandText = $"Starting Pod";
                if (!await UpdateStatusInternal())
                    return;

                if (Pod.LastStatus.Progress >= PodProgress.Running)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is already started");

                if (Pod.LastStatus.Progress < PodProgress.ReadyForInjection)
                    throw new OmniCoreWorkflowException(FailureType.PodStateInvalidForCommand, "Pod is not ready for injection");

                if (Pod.LastStatus.Progress == PodProgress.ReadyForInjection)
                {
                    var parameters = GetStandardParameters();
                    parameters.RepeatFirstPacket = true;
                    parameters.CriticalWithFollowupRequired = true;
                    await SetBasalScheduleInternal(profile, parameters);

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
                    if (!await PerformExchange(request, GetStandardParameters()))
                        return;

                    request = new ErosMessageBuilder().WithInsertCannula().Build();
                    if (!await PerformExchange(request, GetStandardParameters()))
                        return;

                    if (Pod.LastStatus.Progress != PodProgress.Priming)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not start priming the cannula for insertion");

                    Pod.InsertionDate = DateTimeOffset.UtcNow;
                }

                while (Pod.LastStatus.Progress == PodProgress.Priming)
                {
                    var ticks = (int)(Pod.LastStatus.NotDeliveredInsulin / 0.05m);
                    await Task.Delay(ticks * 1000 + 200);

                    if (!await UpdateStatusInternal())
                        return;
                }

                if (Pod.LastStatus.Progress != PodProgress.Running)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not enter the running state");

                Pod.ReservoirUsedForPriming = Pod.LastStatus.DeliveredInsulin;

            }
            catch (Exception e)
            {
                this.Exception = e;
            }
        }

        [method: SuppressMessage("", "CS1998", Justification = "Not implemented")]
        public async Task ConfigureAlerts(AlertConfiguration[] alertConfigurations)
        {
            throw new NotImplementedException();
        }

        public async Task CancelBolus()
        {
            try
            {
                AssertRunningStatus();
                AssertImmediateBolusActive();

                var request = new ErosMessageBuilder().WithCancelBolus().Build();
                if (!await PerformExchange(request, GetStandardParameters()))
                    return;

                if (Pod.LastStatus.BolusState != BolusState.Inactive)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not cancel the bolus");
            }
            catch (Exception e)
            {
                this.Exception = e;
            }
        }

        public async Task CancelTempBasal()
        {
            try
            {
                if (!await UpdateStatusInternal())
                    return;

                AssertRunningStatus();
                AssertImmediateBolusInactive();

                if (Pod.LastStatus.BasalState == BasalState.Temporary)
                {
                    var request = new ErosMessageBuilder().WithCancelTempBasal().Build();
                    if (!await PerformExchange(request, GetStandardParameters()))
                        return;

                    if (Pod.LastStatus.BasalState != BasalState.Scheduled)
                        throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not cancel the temp basal");
                }

                Pod.LastTempBasalResult = null;
            }
            catch (Exception e)
            {
                this.Exception = e;
            }
        }

        public Task StartExtendedBolus(decimal bolusAmount, decimal durationInHours)
        {
            throw new NotImplementedException();
        }

        public Task CancelExtendedBolus()
        {
            throw new NotImplementedException();
        }

        public async Task SetBasalSchedule(IProfile profile)
        {
            try
            {
                if (!await UpdateStatusInternal())
                    return;

                AssertRunningStatus();
                AssertImmediateBolusInactive();

                if (Pod.LastStatus.BasalState == BasalState.Temporary)
                {
                    var cancelReq = new ErosMessageBuilder().WithCancelTempBasal().Build();
                    if (!await PerformExchange(cancelReq, GetStandardParameters()))
                        return;
                }

                if (Pod.LastStatus.BasalState == BasalState.Temporary)
                    throw new OmniCoreWorkflowException(FailureType.PodResponseUnexpected, "Pod did not cancel the temp basal");

                var parameters = GetStandardParameters();
                parameters.RepeatFirstPacket = false;
                parameters.CriticalWithFollowupRequired = false;

                await SetBasalScheduleInternal(profile, parameters);
            }
            catch (Exception e)
            {
                this.Exception = e;
            }
        }

        private async Task SetBasalScheduleInternal(IProfile profile, IMessageExchangeParameters parameters)
        {
            try
            {
                AssertBasalScheduleValid(profile.BasalSchedule);

                var podDate = DateTimeOffset.UtcNow + TimeSpan.FromMinutes(profile.UtcOffset);

                var request = new ErosMessageBuilder()
                    .WithBasalSchedule(profile.BasalSchedule, (ushort)podDate.Hour, (ushort)podDate.Minute, (ushort)podDate.Second)
                    .Build();

                await PerformExchange(request, parameters, (result) =>
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
                this.Exception = e;
            }
        }

        public Task SuspendBasal()
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
