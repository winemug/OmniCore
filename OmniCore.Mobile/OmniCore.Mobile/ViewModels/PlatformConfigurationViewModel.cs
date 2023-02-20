using System;
using System.Threading.Tasks;
using OmniCore.Mobile.Views;
using OmniCore.Services.Interfaces;
using Unity;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public class PlatformConfigurationViewModel : DialogViewModel
    {
        public bool IsAskPermissionsEnabled { get => !_platformInfo.HasAllPermissions; }
        public bool IsOpenBatteryOptimizationsEnabled { get => !_platformInfo.IsExemptFromBatteryOptimizations; }
        public Command AskForPermissions { get; }
        public Command OpenBatteryOptimizationSettings { get; }
        private IPlatformInfo _platformInfo;
        public PlatformConfigurationViewModel()
        {
            AskForPermissions = new Command(RequestPermissionsClicked);
            OpenBatteryOptimizationSettings = new Command(OpenBatteryOptimizationSettingsClicked);
            _platformInfo = DependencyService.Get<IPlatformInfo>();
        }

        protected override async ValueTask OnAppearing()
        {
            // if (PlatformInfo.HasAllPermissions && PlatformInfo.IsExemptFromBatteryOptimizations)
            // {
            //     await NavigationService.NavigateAsync<StartPage>();
            // }
            
            await RaisePropertyChangedAsync();
        }

        private async void RequestPermissionsClicked()
        {
            await _platformInfo.RequestMissingPermissions();
            await RaisePropertyChangedAsync();
        }

        private void OpenBatteryOptimizationSettingsClicked()
        {
            _platformInfo.OpenBatteryOptimizationSettings();
        }
    }
}