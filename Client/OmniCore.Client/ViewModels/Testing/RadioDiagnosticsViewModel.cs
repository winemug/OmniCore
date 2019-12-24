using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces.Platform;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Testing
{
    public class RadioDiagnosticsViewModel : BaseViewModel
    {
        public IRadio Radio { get; set; }
        public ICommand Test1Command { get; set; }
        public RadioDiagnosticsViewModel()
        {
            Test1Command = new Command(async _ =>
            {
                await RunTest1();
            });
        }

        public override async Task Initialize()
        {
        }

        public override async Task Dispose()
        {
        }

        private async Task RunTest1()
        {
            using (var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(60)))
            using (var radioLease = await Radio.Lease(cancellation.Token))
            {
                await radioLease.Initialize(cancellation.Token);
                for (int i = 0; i < 1000; i++)
                {
                    try
                    {
                        var result = await radioLease.DebugGetPacket(60000, cancellation.Token);
                        Debug.WriteLine($"Get result: Rssi: {result.Rssi} Data: {BitConverter.ToString(result.Data)}");
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }
        }
    }
}
