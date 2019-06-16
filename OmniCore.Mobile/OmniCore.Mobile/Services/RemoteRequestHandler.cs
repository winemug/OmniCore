using OmniCore.Mobile.Interfaces;
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
using OmniCore.Model.Interfaces.Data;

namespace OmniCore.Mobile.Services
{
    public class RemoteRequestHandler : IRemoteRequestSubscriber
    {
        public async Task<string> OnRequestReceived(string requestText)
        {
            var logger = DependencyService.Get<IOmniCoreLogger>();
            logger.Debug($"Remote request received: {requestText}");
            var request = RemoteRequest.FromJson(requestText);
            var result = await Execute(request);
            var ret = result.ToJson();
            logger.Debug($"Returning result: {ret}");
            return ret;
        }

        private async Task<RemoteResult> Execute(RemoteRequest request)
        {
            var logger = DependencyService.Get<IOmniCoreLogger>();
            RemoteResult result = null;
            switch (request.Type)
            {
                case RemoteRequestType.Bolus:
                    logger.Debug($"Remote request for bolus: {request.ImmediateUnits} U");
                    result = await Bolus(request.ImmediateUnits);
                    break;
                case RemoteRequestType.CancelBolus:
                    logger.Debug($"Remote request for cancel bolus");
                    result = await CancelBolus();
                    break;
                case RemoteRequestType.CancelTempBasal:
                    logger.Debug($"Remote request for cancel temp basal");
                    result = await CancelTempBasal();
                    break;
                case RemoteRequestType.SetProfile:
                    logger.Debug($"Remote request for set profile: schedule {request.BasalSchedule} utc offset: {request.UtcOffsetMinutes}");
                    result = await SetProfile(request.BasalSchedule, request.UtcOffsetMinutes);
                    break;
                case RemoteRequestType.SetTempBasal:
                    logger.Debug($"Remote request for set temp basal: {request.TemporaryRate} U/h, {request.DurationHours} h");
                    result = await SetTempBasal(request.TemporaryRate, request.DurationHours);
                    break;
                case RemoteRequestType.GetStatus:
                    logger.Debug($"Remote request for get status");
                    result = await GetStatus();
                    break;
            }
            FillResultsToDate(request, result);
                
            return result;
        }

        private bool IsAssigned(IPod pod)
        {
            return (pod != null && pod.Lot.HasValue && pod.Serial.HasValue);
        }

        private RemoteResult GetResult(IPod pod, IConversation conversation)
        {
            var profile = ErosRepository.Instance.GetProfile();

            return new RemoteResult()
            {
                Success = !conversation.Failed,
                PodId = $"L{pod.Lot}T{pod.Serial}R{pod.RadioAddress}",
                ResultDate = new DateTimeOffset(conversation.CurrentExchange.Result.ResultTime.Value).ToUnixTimeMilliseconds(),
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

        private RemoteResult GetResultEstimate(IPod pod)
        {
            pod.LastStatus?.UpdateWithEstimates(pod);
            var profile = ErosRepository.Instance.GetProfile();

            return new RemoteResult()
            {
                Success = true,
                PodId = $"L{pod.Lot}T{pod.Serial}R{pod.RadioAddress}",
                ResultDate = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
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

        private RemoteResult ResultWithProfile()
        {
            var profile = ErosRepository.Instance.GetProfile();

            return new RemoteResult()
            {
                ResultDate = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                BasalSchedule = profile.BasalSchedule,
                UtcOffset = profile.UtcOffset
            };
        }

        private async Task<RemoteResult> CancelBolus()
        {
            var podProvider = App.Instance.PodProvider;
            var podManager = podProvider.PodManager;
            var pod = podManager?.Pod;
            if (IsAssigned(pod))
            {
                using (var conversation = await podManager.StartConversation())
                {
                    await podManager.CancelBolus(conversation).NoSync();
                    return GetResult(pod, conversation);
                }
            }
            return ResultWithProfile();
        }

        private async Task<RemoteResult> Bolus(decimal units)
        {
            var podProvider = App.Instance.PodProvider;
            var podManager = podProvider.PodManager;
            var pod = podManager?.Pod;
            if (IsAssigned(pod))
            {
                using (var conversation = await podManager.StartConversation())
                {
                    await podManager.Bolus(conversation, units, false).NoSync();
                    return GetResult(pod, conversation);
                }
            }
            return ResultWithProfile();
        }

        private async Task<RemoteResult> CancelTempBasal()
        {
            var podProvider = App.Instance.PodProvider;
            var podManager = podProvider.PodManager;
            var pod = podManager?.Pod;
            if (IsAssigned(pod))
            {
                using (var conversation = await podManager.StartConversation())
                {
                    await podManager.CancelTempBasal(conversation).NoSync();
                    return GetResult(pod, conversation);
                }
            }
            return ResultWithProfile();
        }

        private async Task<RemoteResult> SetTempBasal(decimal rate, decimal hours)
        {
            var podProvider = App.Instance.PodProvider;
            var podManager = podProvider.PodManager;
            var pod = podManager?.Pod;
            if (IsAssigned(pod))
            {
                using (var conversation = await podManager.StartConversation())
                {
                    await podManager.SetTempBasal(conversation, rate, hours).NoSync();
                    return GetResult(pod, conversation);
                }
            }
            return ResultWithProfile();
        }

        private async Task<RemoteResult> SetProfile(decimal[] basalSchedule, int utcOffsetMinutes)
        {
            var profile = new ErosProfile()
            {
                Created = DateTime.UtcNow,
                BasalSchedule = basalSchedule,
                UtcOffset = utcOffsetMinutes
            };
            ErosRepository.Instance.Save(profile);

            var podProvider = App.Instance.PodProvider;
            var podManager = podProvider.PodManager;
            var pod = podManager?.Pod;
            if (IsAssigned(pod))
            {
                using (var conversation = await podManager.StartConversation())
                {
                    await podManager.SetBasalSchedule(conversation, profile).NoSync();
                    return GetResult(pod, conversation);
                }
            }
            else
            {
                return ResultWithProfile().WithSuccess();
            }
        }

        private async Task<RemoteResult> GetStatus()
        {
            var podProvider = App.Instance.PodProvider;
            var podManager = podProvider.PodManager;
            var pod = podManager?.Pod;
            if (IsAssigned(pod))
            {
                var ts = DateTime.UtcNow - podManager.Pod.LastStatus.Created;
                if (ts.Minutes > 1)
                {
                    using (var conversation = await podManager.StartConversation())
                    {
                        await podManager.UpdateStatus(conversation).NoSync();
                        return GetResult(pod, conversation);
                    }
                }
                else
                {
                    return GetResultEstimate(pod);
                }
            }
            return ResultWithProfile().WithSuccess();
        }

        private void FillResultsToDate(RemoteRequest request, RemoteResult result)
        {
            var rep = ErosRepository.Instance;
            var unfilteredResults = rep.GetResults(request.LastResultId);

            var list = new List<HistoricalResult>();

            bool? running = null;
            bool nowRunning = false;
            foreach(var oldResult in unfilteredResults)
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
                result.LastResultId = unfilteredResults.Last().Id.Value;
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

        private HistoricalResult GetHistoricalResult(IMessageExchangeResult oldResult, bool running)
        {
            return new HistoricalResult()
            {
                ResultDate = new DateTimeOffset(oldResult.ResultTime.Value).ToUnixTimeMilliseconds(),
                ResultId = oldResult.Id.Value,
                PodRunning = running,
                Type = GetHistoricalType(oldResult.Type),
                Parameters = oldResult.Parameters
            };
        }
    }
}
