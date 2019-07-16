using OmniCore.Mobile.Base.Interfaces;
using OmniCore.Model.Enums;
using OmniCore.Model.Eros;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using OmniCore.Model.Utilities;
using OmniCore.Model.Interfaces;
using Xamarin.Forms;
using OmniCore.Model.Eros.Data;
using Newtonsoft.Json;
using OmniCore.Mobile.Base;
using Microsoft.AppCenter.Crashes;

namespace OmniCore.Mobile.Services
{
    public class RemoteRequestHandler : IRemoteRequestSubscriber
    {
        public async Task<string> OnRequestReceived(string requestText)
        {
            try
            {
                OmniCoreServices.Logger.Debug($"Remote request received: {requestText}");
                var request = RemoteRequest.FromJson(requestText);
                var result = await Execute(request);
                var ret = result.ToJson();
                OmniCoreServices.Logger.Debug($"Returning result: {ret}");
                return ret;
            }
            catch(Exception e)
            {
                OmniCoreServices.Logger.Error($"Error executing request: {requestText}", e);
                Crashes.TrackError(e);
                return null;
            }
        }

        private long GetUnixTime(DateTimeOffset utcDateTime)
        {
            return utcDateTime.ToUnixTimeMilliseconds();
        }

        private async Task<RemoteResult> Execute(RemoteRequest request)
        {
            RemoteResult result = null;
            try
            {
                switch (request.Type)
                {
                    case RemoteRequestType.Bolus:
                        OmniCoreServices.Logger.Debug($"Remote request for bolus: {request.ImmediateUnits} U");
                        result = await Bolus(request.ImmediateUnits);
                        break;
                    case RemoteRequestType.CancelBolus:
                        OmniCoreServices.Logger.Debug($"Remote request for cancel bolus");
                        result = await CancelBolus();
                        break;
                    case RemoteRequestType.CancelTempBasal:
                        OmniCoreServices.Logger.Debug($"Remote request for cancel temp basal");
                        result = await CancelTempBasal();
                        break;
                    case RemoteRequestType.SetProfile:
                        OmniCoreServices.Logger.Debug($"Remote request for set profile: schedule {request.BasalSchedule} utc offset: {request.UtcOffsetMinutes}");
                        result = await SetProfile(request.BasalSchedule, request.UtcOffsetMinutes);
                        break;
                    case RemoteRequestType.SetTempBasal:
                        OmniCoreServices.Logger.Debug($"Remote request for set temp basal: {request.TemporaryRate} U/h, {request.DurationHours} h");
                        result = await SetTempBasal(request.TemporaryRate, request.DurationHours);
                        break;
                    case RemoteRequestType.GetStatus:
                        OmniCoreServices.Logger.Debug($"Remote request for get status");
                        result = await GetStatus();
                        break;
                }
                await FillResultsToDate(request, result);
            }
            catch(Exception e)
            {
                OmniCoreServices.Logger.Error($"Error executing request", e);
                Crashes.TrackError(e);
            }
            return result;
        }

        private bool IsAssigned(IPod pod)
        {
            return (pod != null && pod.Lot.HasValue && pod.Serial.HasValue);
        }

        private async Task<RemoteResult> GetResult(IPod pod, IConversation conversation)
        {
            var repo = await ErosRepository.GetInstance();
            var profile = await repo.GetProfile();

            return new RemoteResult()
            {
                Success = !conversation.Failed,
                PodId = $"L{pod.Lot}T{pod.Serial}R{pod.RadioAddress}",
                ResultDate = GetUnixTime(conversation.CurrentExchange.Result.ResultTime.Value),
                InsulinCanceled = pod.LastStatus?.NotDeliveredInsulin ?? 0,
                PodRunning = (pod.LastStatus != null && pod.LastStatus.Progress.HasValue &&
                            pod.LastStatus.Progress >= PodProgress.Running &&
                            pod.LastStatus.Progress <= PodProgress.RunningLow &&
                            (pod.LastFault == null || pod.LastFault.FaultCode == 9)),
                ReservoirLevel = pod.LastStatus?.Reservoir ?? 0,
                BasalSchedule = pod.LastBasalSchedule?.BasalSchedule ?? profile.BasalSchedule,
                UtcOffset = pod.LastBasalSchedule?.UtcOffset ?? profile.UtcOffset
            };
        }

        private async Task<RemoteResult> GetResultEstimate(IPod pod)
        {
            var repo = await ErosRepository.GetInstance();
            pod.LastStatus?.UpdateWithEstimates(pod);
            var profile = await repo.GetProfile();

            return new RemoteResult()
            {
                Success = true,
                PodId = $"L{pod.Lot}T{pod.Serial}R{pod.RadioAddress}",
                ResultDate = GetUnixTime(DateTimeOffset.UtcNow),
                InsulinCanceled = pod.LastStatus?.NotDeliveredInsulin ?? 0,
                ReservoirLevel = pod.LastStatus?.ReservoirEstimate ?? 0,
                PodRunning = (pod.LastStatus != null && pod.LastStatus.Progress.HasValue &&
                            pod.LastStatus.Progress >= PodProgress.Running &&
                            pod.LastStatus.Progress <= PodProgress.RunningLow &&
                            (pod.LastFault == null || pod.LastFault.FaultCode == 9)),
                BasalSchedule = pod.LastBasalSchedule?.BasalSchedule ?? profile.BasalSchedule,
                UtcOffset = pod.LastBasalSchedule?.UtcOffset ?? profile.UtcOffset
            };
        }

        private async Task<RemoteResult> ResultWithProfile()
        {
            var repo = await ErosRepository.GetInstance();
            var profile = await repo.GetProfile();

            return new RemoteResult()
            {
                ResultDate = GetUnixTime(DateTimeOffset.UtcNow),
                BasalSchedule = profile.BasalSchedule,
                UtcOffset = profile.UtcOffset
            };
        }

        private async Task<RemoteResult> CancelBolus()
        {
            var podProvider = App.Instance.PodProvider;
            var pod = podProvider.SinglePod;
            if (IsAssigned(pod))
            {
                using (var conversation = await pod.StartConversation(App.Instance.ExchangeProvider, "Cancel Bolus", source: RequestSource.AndroidAPS))
                {
                    await pod.CancelBolus(conversation);
                    return await GetResult(pod, conversation);
                }
            }
            return await ResultWithProfile();
        }

        private async Task<RemoteResult> Bolus(decimal units)
        {
            var podProvider = App.Instance.PodProvider;
            var pod = podProvider.SinglePod;
            if (IsAssigned(pod))
            {
                using (var conversation = await pod.StartConversation(App.Instance.ExchangeProvider,
                    $"Bolus {units:F2}U", source: RequestSource.AndroidAPS))
                {
                    await pod.Bolus(conversation, units, false);
                    return await GetResult(pod, conversation);
                }
            }
            return await ResultWithProfile();
        }

        private async Task<RemoteResult> CancelTempBasal()
        {
            var podProvider = App.Instance.PodProvider;
            var pod = podProvider.SinglePod;
            if (IsAssigned(pod))
            {
                using (var conversation = await pod.StartConversation(App.Instance.ExchangeProvider, 
                    "Cancel Temp Basal", source: RequestSource.AndroidAPS))
                {
                    await pod.CancelTempBasal(conversation);
                    return await GetResult(pod, conversation);
                }
            }
            return await ResultWithProfile();
        }

        private async Task<RemoteResult> SetTempBasal(decimal rate, decimal hours)
        {
            var podProvider = App.Instance.PodProvider;
            var pod = podProvider.SinglePod;
            if (IsAssigned(pod))
            {
                using (var conversation = await pod.StartConversation(App.Instance.ExchangeProvider, 
                    $"Set Temp Basal {rate:F2}U/hr for {hours:F1}h",
                    source: RequestSource.AndroidAPS))
                {
                    await pod.SetTempBasal(conversation, rate, hours);
                    return await GetResult(pod, conversation);
                }
            }
            return await ResultWithProfile();
        }

        private async Task<RemoteResult> SetProfile(decimal[] basalSchedule, int utcOffsetMinutes)
        {
            var profile = new ErosProfile()
            {
                Created = DateTimeOffset.UtcNow,
                BasalSchedule = basalSchedule,
                UtcOffset = utcOffsetMinutes
            };
            var repo = await ErosRepository.GetInstance();
            await repo.Save(profile);

            var podProvider = App.Instance.PodProvider;
            var pod = podProvider.SinglePod;
            if (IsAssigned(pod))
            {
                using (var conversation = await pod.StartConversation(App.Instance.ExchangeProvider, 
                    $"Set Basal Schedule", source: RequestSource.AndroidAPS))
                {
                    await pod.SetBasalSchedule(conversation, profile);
                    return await GetResult(pod, conversation);
                }
            }
            else
            {
                return (await ResultWithProfile()).WithSuccess();
            }
        }

        private async Task<RemoteResult> GetStatus()
        {
            var podProvider = App.Instance.PodProvider;
            var pod = podProvider.SinglePod;
            if (IsAssigned(pod))
            {
                var ts = DateTimeOffset.UtcNow - pod.LastStatus?.Created;
                if (ts == null || ts.Value.Minutes > 20)
                {
                    using (var conversation = await pod.StartConversation(App.Instance.ExchangeProvider, 
                        "Update Status", source: RequestSource.AndroidAPS))
                    {
                        await pod.UpdateStatus(conversation);
                        return await GetResult(pod, conversation);
                    }
                }
                else
                {
                    return await GetResultEstimate(pod);
                }
            }
            return (await ResultWithProfile()).WithSuccess();
        }

        private async Task FillResultsToDate(RemoteRequest request, RemoteResult result)
        {
            try
            {
                var repo = await ErosRepository.GetInstance();
                var unfilteredResults = await repo.GetHistoricalResultsForRemoteApp(request.LastResultDateTime);

                var list = new List<HistoricalResult>();

                bool? running = null;
                bool nowRunning = false;
                foreach (var oldResult in unfilteredResults)
                {
                    if (oldResult.Fault?.FaultCode != null)
                    {
                        nowRunning = (oldResult.Fault.FaultCode == 9);
                    }
                    if (oldResult.Status?.Faulted != null)
                    {
                        nowRunning = !oldResult.Status.Faulted.Value;
                    }
                    if (oldResult.Status?.Progress != null)
                    {
                        nowRunning = (oldResult.Status.Progress == PodProgress.Running
                            || oldResult.Status.Progress == PodProgress.RunningLow);
                    }

                    if (!running.HasValue || running.Value != nowRunning)
                    {
                        list.Add(GetHistoricalResult(oldResult, nowRunning));
                        running = nowRunning;
                    }
                    else
                    {
                        switch (oldResult.Type)
                        {
                            case RequestType.SetBasalSchedule:
                            case RequestType.SetTempBasal:
                            case RequestType.CancelTempBasal:
                            case RequestType.Bolus:
                            case RequestType.CancelBolus:
                            case RequestType.StartExtendedBolus:
                            case RequestType.StopExtendedBolus:
                                list.Add(GetHistoricalResult(oldResult, nowRunning));
                                break;
                            default:
                                break;
                        }
                    }
                }

                result.ResultsToDate = list.ToArray();
                if (unfilteredResults.Count > 0)
                    result.LastResultDateTime = GetUnixTime(unfilteredResults.Last().ResultTime.Value);
                else
                    result.LastResultDateTime = request.LastResultDateTime;
            }
            catch(Exception e)
            {
                OmniCoreServices.Logger.Error($"Error getting results to date for LastResultDate={request.LastResultDateTime}", e);
                Crashes.TrackError(e);
            }
        }

        private HistoricalResultType GetHistoricalType(RequestType type)
        {
            switch (type)
            {
                case RequestType.SetBasalSchedule:
                    return HistoricalResultType.SetBasalSchedule;
                case RequestType.SetTempBasal:
                    return HistoricalResultType.SetTempBasal;
                case RequestType.CancelTempBasal:
                    return HistoricalResultType.CancelTempBasal;
                case RequestType.Bolus:
                    return HistoricalResultType.Bolus;
                case RequestType.CancelBolus:
                    return HistoricalResultType.CancelBolus;
                case RequestType.StartExtendedBolus:
                    return HistoricalResultType.StartExtendedBolus;
                case RequestType.StopExtendedBolus:
                    return HistoricalResultType.StopExtendedBolus;
                default:
                    return HistoricalResultType.Status;
            }
        }

        private HistoricalResult GetHistoricalResult(ErosMessageExchangeResult oldResult, bool running)
        {
            var hr = new HistoricalResult()
            {
                ResultDate = GetUnixTime(oldResult.ResultTime.Value),
                ResultId = oldResult.Id.Value,
                PodRunning = running,
                Type = GetHistoricalType(oldResult.Type),
                Parameters = oldResult.Parameters
            };

            if (hr.Type == HistoricalResultType.CancelBolus)
            {
                hr.Parameters = JsonConvert.SerializeObject(new { NotDeliveredInsulin = oldResult.Status.NotDeliveredInsulin.Value });
            }

            return hr;
        }
    }
}
