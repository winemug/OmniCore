using System.Diagnostics;
using Dapper;
using OmniCore.Services;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public class BluetoothTestViewModel : BaseViewModel
    {
        private readonly IForegroundServiceHelper _foregroundServiceHelper;
        private IPlatformInfo _platformInfo;

        private int dac;

        public BluetoothTestViewModel()
        {
            _foregroundServiceHelper = DependencyService.Get<IForegroundServiceHelper>();
            _platformInfo = DependencyService.Get<IPlatformInfo>();
            StartCommand = new Command(StartClicked);
            StopCommand = new Command(StopClicked);
            DoCommand = new Command(DoClicked);
            CopyCommand = new Command(CopyClicked);
        }

        public Command StartCommand { get; }
        public Command StopCommand { get; }
        public Command DoCommand { get; }

        public Command CopyCommand { get; }

        public string RssiText { get; private set; }

        [Unity.Dependency] public RadioService RadioService { get; set; }

        [Unity.Dependency] public PodService PodService { get; set; }

        [Unity.Dependency] public DataService DataService { get; set; }

        [Unity.Dependency] public AmqpService AmqpService { get; set; }

        private void StartClicked()
        {
            _foregroundServiceHelper.StartForegroundService();
        }

        private async void CopyClicked()
        {
            using (var conn = await DataService.GetConnectionAsync())
            {
                var res1 = await conn.QueryAsync("SELECT * FROM pod");
                var res2 = await conn.QueryAsync("SELECT * FROM pod_message");
                Debug.WriteLine("");
            }
        }

        private async void DoClicked()
        {
            var pod = await PodService.GetPodAsync();
            using (var podConn = await PodService.GetConnectionAsync(pod))
            {
                var response = await podConn.Deactivate();
                Debug.WriteLine($"result: {response}");
            }

            Debug.WriteLine(pod);

            await AmqpService.PublishMessage(new AmqpMessage { Text = $"message #{dac}" });
            dac++;
        }

        private void StopClicked()
        {
            _foregroundServiceHelper.StopForegroundService();
        }
    }
}