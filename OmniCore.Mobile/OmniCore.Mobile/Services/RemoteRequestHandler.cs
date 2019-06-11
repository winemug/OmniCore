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
            var podStatus = CreateCurrentStatus();

            switch (request.Type.Value)
            {
                case RemoteRequestType.Bolus:
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

        private RemoteResultPodStatus CreateCurrentStatus()
        {
            var status = new RemoteResultPodStatus() { PodRunning = false,
                LastUpdated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            var podManager = App.Instance.PodProvider.Current;
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
                    if (pod.LastStatus.Id.HasValue)
                        status.ResultId = pod.LastStatus.Id.Value;

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
