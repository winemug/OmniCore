using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using OmniCore.Mobile.Annotations;
using OmniCore.Services;
using OmniCore.Services.Interfaces;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions.Extensions;
using Polly;
using Unity;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public class BluetoothTestViewModel : BaseViewModel
    {
        public Command StartCommand { get; }
        public Command StopCommand { get; }
        public Command DoCommand { get; }
        
        public Command CopyCommand { get; }
        
        public string RssiText { get; private set; }

        [Unity.Dependency]
        public RadioService RadioService { get; set; }       
        [Unity.Dependency]
        public PodService PodService { get; set; }
        [Unity.Dependency]
        public DataService DataService { get; set; }
        [Unity.Dependency]
        public AmqpService AmqpService { get; set; }
        
        private IForegroundServiceHelper _foregroundServiceHelper;
        private IPlatformInfo _platformInfo;

        public BluetoothTestViewModel()
        {
            _foregroundServiceHelper = DependencyService.Get<IForegroundServiceHelper>();
            _platformInfo = DependencyService.Get<IPlatformInfo>();
            StartCommand = new Command(StartClicked);
            StopCommand = new Command(StopClicked);
            DoCommand = new Command(DoClicked);
            CopyCommand = new Command(CopyClicked);
        }

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

        private int dac = 0;
        private async void DoClicked()
        {
            var pod = await PodService.GetPodAsync();
            using (var podConn = await PodService.GetConnectionAsync(pod))
            {
                Debug.WriteLine($"Update Status");
                var response = await podConn.UpdateStatus();
            }
            Debug.WriteLine(pod);

            await AmqpService.PublishMessage(new AmqpMessage() { Text = $"message #{dac}" });
            dac++;
        }

        private void StopClicked()
        {
            _foregroundServiceHelper.StopForegroundService();
        }
    }
}