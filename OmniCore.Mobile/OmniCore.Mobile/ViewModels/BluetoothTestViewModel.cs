using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        
        public string RssiText { get; private set; }

        private ICoreService _coreService;
        private RadioService _radioService;
        private PodService _podService;
        private IForegroundServiceHelper _foregroundServiceHelper;

        public BluetoothTestViewModel()
        {
            StartCommand = new Command(StartClicked);
            StopCommand = new Command(StopClicked);
            DoCommand = new Command(DoClicked);
            _coreService = App.Container.Resolve<ICoreService>();
            _radioService = App.Container.Resolve<RadioService>();
            _podService = App.Container.Resolve<PodService>();
            _foregroundServiceHelper = App.Container.Resolve<IForegroundServiceHelper>();
        }

        protected override async Task OnPageShownAsync()
        {
            _foregroundServiceHelper.StartForegroundService();
        }

        private void StartClicked()
        {
            _foregroundServiceHelper.StartForegroundService();
        }

        private async void DoClicked()
        {
            var pod = await _podService.GetPodAsync();
            using (var podConn = await _podService.GetConnectionAsync(pod))
            {
                Debug.WriteLine($"Update Status");
                var response = await podConn.UpdateStatus();
            }
            Debug.WriteLine(pod);
        }

        private void StopClicked()
        {
            _foregroundServiceHelper.StopForegroundService();
        }
    }
}