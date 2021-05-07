using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities.Extensions;

namespace OmniCore.Client.ViewModels.Test
{
    public class TestLogViewModel : BaseViewModel
    {
        public string RadioEventLog { get; set; }
        public TestLogViewModel(IClient client) : base(client)
        {
            WhenPageAppears().Subscribe(async _ =>
            {
                RadioEventLog = await GetEventLog();
            });
        }

        private async Task<string> GetEventLog()
        {
            var api = await Client.GetServiceApi(CancellationToken.None);
            var activePods = await api.PodService.ActivePods(CancellationToken.None);
            var testPod = activePods.FirstOrDefault();
            if (testPod == null)
                return "No active test pod";

            using var context = await api.RepositoryService.GetContextReadOnly(CancellationToken.None);
            var sb = new StringBuilder();
            foreach (var radio in testPod.Entity.PodRadios.Select(pr => pr.Radio))
            {
                sb.AppendLine($"MAC: {radio.DeviceUuid.AsMacAddress()}");
                sb.AppendLine($"Name: {radio.DeviceName}");

                foreach (var re in context.RadioEvents.Where(re => re.Radio.Id == radio.Id))
                {
                    sb.AppendLine($"{re.Created.ToLocalTime()} {re.EventType} {re.Rssi} {re.Text}");
                }
            }

            return sb.ToString();
        }
    }
}