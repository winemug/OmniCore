using OmniCore.Mobile.Interfaces;
using OmniCore.Model.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Mobile.Services
{
    public class RemoteRequestHandler : IRemoteRequestSubscriber
    {
        public RemoteRequestHandler()
        {

        }

        public async Task<string> OnRequestReceived(string requestText)
        {
            var result = new RemoteResult();
            var request = RemoteRequest.FromJson(requestText);

            if (request.Type.HasValue)
            {
                await Execute(request, result);
            }

            return result.ToJson();
        }

        private async Task Execute(RemoteRequest request, RemoteResult result)
        {
            var podProvider = App.Instance.PodProvider;
            var podManager = podProvider.PodManager;

            using (var conversation = await podManager.StartConversation())
            {
                if (podManager.Pod.LastStatus == null || podManager.Pod.LastStatus.Progress < PodProgress.PairingSuccess)
                    await Task.Run(async () => await podManager.UpdateStatus(conversation).ConfigureAwait(false));
            }

            switch (request.Type.Value)
            {
                case RemoteRequestType.Bolus:
                    result.Status = CreateFromCurrentStatus();
                    result.Success = false;
                    break;
                case RemoteRequestType.CancelBolus:
                    result.Success = false;
                    break;
                case RemoteRequestType.CancelTempBasal:
                    result.Success = false;
                    break;
                case RemoteRequestType.SetBasalSchedule:
                    result.Success = false;
                    break;
                case RemoteRequestType.SetTempBasal:
                    result.Success = false;
                    break;
                case RemoteRequestType.UpdateStatus:
                    break;
            }
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
