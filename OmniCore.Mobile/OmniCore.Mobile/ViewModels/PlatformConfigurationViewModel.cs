using System;
using System.Threading.Tasks;
using OmniCore.Services.Interfaces;
using Unity;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public class PlatformConfigurationViewModel : BaseViewModel
    {
        public bool IsAskPermissionsEnabled { get => !PlatformInfo.HasAllPermissions; }
        public bool IsOpenBatteryOptimizationsEnabled { get => !PlatformInfo.IsExemptFromBatteryOptimizations; }
        public Command AskForPermissions { get; }
        public Command OpenBatteryOptimizationSettings { get; }

        private IPlatformInfo PlatformInfo;
        public PlatformConfigurationViewModel()
        {
            PlatformInfo = App.Container.Resolve<IPlatformInfo>();
            AskForPermissions = new Command(RequestPermissionsClicked);
            OpenBatteryOptimizationSettings = new Command(OpenBatteryOptimizationSettingsClicked);
        }

        protected override async Task PageShownAsync()
        {
            if (PlatformInfo.HasAllPermissions && PlatformInfo.IsExemptFromBatteryOptimizations)
            {
                // go back to start
                await Shell.Current.GoToAsync($"//StartPage");
            }
        }

        private async void RequestPermissionsClicked()
        {
            await PlatformInfo.RequestMissingPermissions();
            base.OnPropertyChanged("IsAskPermissionsEnabled");
        }

        private void OpenBatteryOptimizationSettingsClicked()
        {
            PlatformInfo.OpenBatteryOptimizationSettings();
        }
    }
}