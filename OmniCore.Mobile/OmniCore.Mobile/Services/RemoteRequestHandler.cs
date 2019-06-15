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

namespace OmniCore.Mobile.Services
{
    public class RemoteRequestHandler : IRemoteRequestSubscriber
    {
        public RemoteRequestHandler()
        {
        }

        public async Task<string> OnRequestReceived(string requestText)
        {
            var logger = DependencyService.Get<IOmniCoreLogger>();
            logger.Debug($"Remote request received: {requestText}");
            var request = RemoteRequest.FromJson(requestText);
            var result = new RemoteResult();

            var podProvider = App.Instance.PodProvider;
            var podManager = podProvider.PodManager;

            if (podManager == null || podManager.Pod.LastStatus == null ||
                podManager.Pod.LastStatus.Progress < PodProgress.Running ||
                podManager.Pod.LastStatus.Progress > PodProgress.RunningLow ||
                (podManager.Pod.LastFault != null && podManager.Pod.LastFault.FaultCode != 9))
            {
                result.PodRunning = false;
            }

            if (request.Type.HasValue)
            {
                if (result.PodRunning)
                    await Execute(request, result);
                else
                {
                    result.Success = false;
                    result.ResultId = 0;
                }
            }
            else
            {
                result.Status = CreateFromCurrentStatus(result);
                result.Success = true;
            }

            if (request.LastResultId.HasValue)
            {
                result.ResultsToDate = GetResultsToDate(request.LastResultId.Value);
            }

            var ret = result.ToJson();
            logger.Debug($"Returning result: {ret}");
            return ret;
        }

        private async Task Execute(RemoteRequest request, RemoteResult result)
        {
            var logger = DependencyService.Get<IOmniCoreLogger>();

            switch (request.Type.Value)
            {
                case RemoteRequestType.Bolus:
                    logger.Debug($"Remote request for bolus: {request.ImmediateUnits} U");
                    await Bolus(request.ImmediateUnits.Value, result);
                    break;
                case RemoteRequestType.CancelBolus:
                    logger.Debug($"Remote request for cancel bolus");
                    await CancelBolus(result);
                    break;
                case RemoteRequestType.CancelTempBasal:
                    logger.Debug($"Remote request for cancel temp basal");
                    await CancelTempBasal(result);
                    break;
                case RemoteRequestType.SetBasalSchedule:
                    logger.Debug($"Remote request for set basal schedule: schedule {request.BasalSchedule} utc offset: {request.UtcOffsetMinutes}");
                    await SetBasalSchedule(request.BasalSchedule, request.UtcOffsetMinutes.Value, result);
                    break;
                case RemoteRequestType.SetTempBasal:
                    logger.Debug($"Remote request for set temp basal: {request.TemporaryRate} U/h, {request.DurationHours} h");
                    await SetTempBasal(request.TemporaryRate.Value, request.DurationHours.Value, result);
                    break;
                case RemoteRequestType.UpdateStatus:
                    logger.Debug($"Remote request for update status");
                    await UpdateStatus(request.StatusRequestType ?? 0, result);
                    break;
                default:
                    break;
            }
        }

        private async Task CancelBolus(RemoteResult result)
        {
            var podProvider = App.Instance.PodProvider;
            var podManager = podProvider.PodManager;
            using (var conversation = await podManager.StartConversation())
            {
                await podManager.CancelBolus(conversation).NoSync();
                result.ResultId = conversation.CurrentExchange.Result.Id;
                result.Success = !conversation.Failed;
            }
        }

        private async Task Bolus(decimal units, RemoteResult result)
        {
            var podProvider = App.Instance.PodProvider;
            var podManager = podProvider.PodManager;
            using (var conversation = await podManager.StartConversation())
            {
                await podManager.Bolus(conversation, units, false).NoSync();
                result.ResultId = conversation.CurrentExchange.Result.Id;
                result.Success = !conversation.Failed;
            }
        }

        private async Task CancelTempBasal(RemoteResult result)
        {
            var podProvider = App.Instance.PodProvider;
            var podManager = podProvider.PodManager;
            using (var conversation = await podManager.StartConversation())
            {
                await podManager.CancelTempBasal(conversation).NoSync();
                result.ResultId = conversation.CurrentExchange.Result.Id;
                result.Success = !conversation.Failed;
            }
        }

        private async Task SetTempBasal(decimal rate, decimal hours, RemoteResult result)
        {
            var podProvider = App.Instance.PodProvider;
            var podManager = podProvider.PodManager;
            using (var conversation = await podManager.StartConversation())
            {
                await podManager.SetTempBasal(conversation, rate, hours).NoSync();
                result.ResultId = conversation.CurrentExchange.Result.Id;
                result.Success = !conversation.Failed;
            }
        }

        private async Task SetBasalSchedule(decimal[] basalSchedule, int utcOffsetMinutes, RemoteResult result)
        {
            var podProvider = App.Instance.PodProvider;
            var podManager = podProvider.PodManager;
            using (var conversation = await podManager.StartConversation())
            {
                await podManager.SetBasalSchedule(conversation, basalSchedule, utcOffsetMinutes).NoSync();
                result.ResultId = conversation.CurrentExchange.Result.Id;
                result.Success = !conversation.Failed;
            }
        }

        private async Task UpdateStatus(int reqType, RemoteResult result)
        {
            var podProvider = App.Instance.PodProvider;
            var podManager = podProvider.PodManager;
            
            var ts = DateTime.UtcNow - podManager.Pod.LastStatus.Created;
            if (ts.Minutes > 1)
            {
                using (var conversation = await podManager.StartConversation())
                {
                    await podManager.UpdateStatus(conversation).NoSync();
                    result.ResultId = conversation.CurrentExchange.Result.Id;
                    result.Success = !conversation.Failed;
                }
            }
            else
            {
                result.Status = CreateFromCurrentStatus(result);
                result.Success = true;
            }
        }

        private HistoricalResult[] GetResultsToDate(long lastResultId)
        {
            var rep = ErosRepository.Instance;
            var unfiltered = rep.GetResults(lastResultId);

            var list = new List<HistoricalResult>();
            bool? LastFaulted = null;
            PodProgress? LastProgress = null;
            BasalState? LastBasalState = null;
            BolusState? LastBolusState = null;

            int startIndex = 0;
            if (lastResultId > 0 && unfiltered.Count > 0 && unfiltered[0].Id == lastResultId)
            {
                startIndex++;
                var lastResult = unfiltered[0];
                if (lastResult.Status != null)
                {
                    LastFaulted = lastResult.Status.Faulted;
                    LastProgress = lastResult.Status.Progress;
                    LastBasalState = lastResult.Status.BasalState;
                    LastBolusState = lastResult.Status.BolusState;
                }
                else if (lastResult.Fault != null)
                {
                    LastFaulted = lastResult.Fault.FaultCode != 9;
                }
            }

            for(int i=startIndex; i < unfiltered.Count; i++)
            {
                var result = unfiltered[i];
            }

            return list.ToArray();
        }

        private RemoteResultPodStatus CreateFromCurrentStatus(RemoteResult result)
        {
            var status = new RemoteResultPodStatus() { 
                LastUpdated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            var podManager = App.Instance.PodProvider.PodManager;
            if (podManager != null)
            {
                var pod = podManager.Pod;

                status.LastUpdated = new DateTimeOffset(pod.Created).ToUnixTimeMilliseconds();
                if (pod.Lot.HasValue && pod.Serial.HasValue)
                {
                    result.PodId = $"L{pod.Lot}T{pod.Serial}R{pod.RadioAddress}";
                }

                if (pod.LastBasalSchedule != null)
                {
                    status.BasalSchedule = pod.LastBasalSchedule.BasalSchedule;
                    status.UtcOffset = pod.LastBasalSchedule.UtcOffset;
                }

                if (pod.LastStatus != null)
                {
                    status.LastUpdated = new DateTimeOffset(pod.Created).ToUnixTimeMilliseconds();
                    result.ResultId = pod.LastStatus.Id ?? 0;
                    status.ReservoirLevel = (double)pod.LastStatus.Reservoir;
                    status.InsulinCanceled = (double)pod.LastStatus.NotDeliveredInsulin;
                }
            }
            return status;
        }
    }
}
