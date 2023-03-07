using System.Threading.Tasks;
using OmniCore.Services.Interfaces;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public class PlatformConfigurationViewModel : DialogViewModel
    {
        private readonly IPlatformInfo _platformInfo;

        public PlatformConfigurationViewModel()
        {
            AskForPermissions = new Command(RequestPermissionsClicked);
            OpenBatteryOptimizationSettings = new Command(OpenBatteryOptimizationSettingsClicked);
            _platformInfo = DependencyService.Get<IPlatformInfo>();
        }

        public bool IsAskPermissionsEnabled => !_platformInfo.HasAllPermissions;
        public bool IsOpenBatteryOptimizationsEnabled => !_platformInfo.IsExemptFromBatteryOptimizations;
        public Command AskForPermissions { get; }
        public Command OpenBatteryOptimizationSettings { get; }

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