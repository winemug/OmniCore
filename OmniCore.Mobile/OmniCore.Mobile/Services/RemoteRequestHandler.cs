using OmniCore.Mobile.Interfaces;
using OmniCore.Model.Enums;
using OmniCore.Model.Eros;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

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

            if (request.Type.HasValue)
            {
                var result = await Execute(request);
                return result.ToJson();
            }
            return null;
        }

        private async Task<RemoteResult> Execute(RemoteRequest request)
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
                    return await UpdateStatus(request.StatusRequestType ?? 0);
            }
            return null;
        }

        private async Task<RemoteResult> UpdateStatus(int reqType)
        {
            var podProvider = App.Instance.PodProvider;
            var podManager = podProvider.PodManager;

            var result = new RemoteResult();
            using (var conversation = await podManager.StartConversation())
            {
                if (podManager.Pod.LastStatus == null || podManager.Pod.LastStatus.Progress < PodProgress.PairingSuccess)
                    await Task.Run(async () => await podManager.UpdateStatus(conversation).ConfigureAwait(false));

                result.Status = CreateFromCurrentStatus();
                result.Success = !conversation.Failed;
                // result.RequestsToDate = GetRequestsToDate(int fromRequestId);
            }
            return result;
        }

        private RemoteRequest[] GetRequestsToDate(int earliestRequestId)
        {
            var rep = ErosRepository.Instance;
            var unfiltered = rep.GetResults(earliestRequestId);
            return null;
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
