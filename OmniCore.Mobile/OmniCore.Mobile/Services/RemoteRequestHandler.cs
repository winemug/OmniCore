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

namespace OmniCore.Mobile.Services
{
    public class RemoteRequestHandler : IRemoteRequestSubscriber
    {
        public RemoteRequestHandler()
        {

        }

        public async Task<string> OnRequestReceived(string requestText)
        {
            var request = RemoteRequest.FromJson(requestText);
            var result = new RemoteResult();

            if (request.Type.HasValue)
            {
                await Execute(request, result);
            }
            else
            {
                result.Status = CreateFromCurrentStatus();
            }

            if (request.LastResultId.HasValue)
            {
                result.ResultsToDate = GetResultsToDate(request.LastResultId.Value);
            }
            return result.ToJson();
        }

        private async Task Execute(RemoteRequest request, RemoteResult result)
        {
            switch (request.Type.Value)
            {
                //case RemoteRequestType.Bolus:
                //    break;
                //case RemoteRequestType.CancelBolus:
                //    result.Success = false;
                //    break;
                //case RemoteRequestType.CancelTempBasal:
                //    result.Success = false;
                //    break;
                //case RemoteRequestType.SetBasalSchedule:
                //    result.Success = false;
                //    break;
                //case RemoteRequestType.SetTempBasal:
                //    result.Success = false;
                //    break;
                case RemoteRequestType.UpdateStatus:
                    await UpdateStatus(request.StatusRequestType ?? 0, result);
                    break;
            }
        }

        private async Task UpdateStatus(int reqType, RemoteResult result)
        {
            var podProvider = App.Instance.PodProvider;
            var podManager = podProvider.PodManager;
            

            if (podManager == null || podManager.Pod.LastStatus == null ||
                podManager.Pod.LastStatus.Progress < PodProgress.Running ||
                podManager.Pod.LastStatus.Progress > PodProgress.RunningLow)
            {
                result.Status = new RemoteResultPodStatus()
                {
                    PodRunning = false,
                    LastUpdated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };
                result.Success = true;
            }
            else
            {
                var ts = DateTime.UtcNow - podManager.Pod.LastStatus.Created;
                if (ts.Minutes > 1)
                {
                    using (var conversation = await podManager.StartConversation())
                    {
                        await podManager.UpdateStatus(conversation).NoSync();
                        result.Success = !conversation.Failed;
                    }
                }
                else
                {
                    result.Status = CreateFromCurrentStatus();
                    result.Success = true;
                }
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

        private RemoteResultPodStatus CreateFromCurrentStatus()
        {
            var status = new RemoteResultPodStatus() { PodRunning = false,
                LastUpdated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            var podManager = App.Instance.PodProvider.PodManager;
            if (podManager != null)
            {
                var pod = podManager.Pod;

                status.LastUpdated = new DateTimeOffset(pod.Created).ToUnixTimeMilliseconds();
                if (pod.Lot.HasValue && pod.Id.HasValue)
                {
                    status.PodId = $"L{pod.Lot}T{pod.Id}R{pod.RadioAddress}";
                }

                if (pod.LastBasalSchedule != null)
                {
                    status.BasalSchedule = pod.LastBasalSchedule.BasalSchedule;
                    status.UtcOffset = pod.LastBasalSchedule.UtcOffset;
                }

                if (pod.LastStatus != null)
                {
                    status.LastUpdated = new DateTimeOffset(pod.Created).ToUnixTimeMilliseconds();
                    status.ResultId = pod.LastStatus.Id ?? 0;
                    status.PodRunning = pod.LastStatus.Progress >= PodProgress.Running && pod.LastStatus.Progress <= PodProgress.RunningLow
                        && pod.LastFault == null;
                    status.ReservoirLevel = (double)pod.LastStatus.Reservoir;
                    status.InsulinCanceled = (double)pod.LastStatus.NotDeliveredInsulin;

                    if (status.PodRunning)
                        status.StatusText = $"Pod running";
                    else
                        status.StatusText = $"Pod inactive";
                }
            }
            return status;
        }
    }
}
