using System;
using System.Diagnostics;
using OmniCore.Mobile.Services;
using OmniCore.Mobile.Views;
using OmniCore.Services.Interfaces;
using Unity;
using Xamarin.Forms;

namespace OmniCore.Mobile
{
    public partial class App : Application
    {
        private readonly NavigationService _navigationService;
        private readonly IPlatformInfo _platformInfo;

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            InitializeComponent();
            var container = new UnityContainer();
            Initializer.RegisterTypes(container);
            _navigationService = container.Resolve<NavigationService>();
            DependencyService.Get<IForegroundServiceHelper>().Service = container.Resolve<IForegroundService>();
            _platformInfo = DependencyService.Get<IPlatformInfo>();

            var shell = new AppShell();
            _navigationService.SetShellInstance(shell);
            MainPage = shell;
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Trace.WriteLine($"Unhandled exception: {e.ExceptionObject}");
        }

        protected override async void OnStart()
        {
            Debug.WriteLine("App On Start");

            if (!_platformInfo.IsExemptFromBatteryOptimizations || !_platformInfo.HasAllPermissions)
                await _navigationService.NavigateAsync<PlatformConfigurationPage>();

            await _navigationService.NavigateAsync<BluetoothTestPage>();

            // await NavigationService.NavigateAsync<AmqpTestPage>();
        }

        protected override async void OnSleep()
        {
            Debug.WriteLine("App On Sleep");
            await _navigationService.OnSleepAsync();
            base.OnSleep();
        }

        protected override async void OnResume()
        {
            Debug.WriteLine("App On Resume");
            await _navigationService.OnResumeAsync();
            base.OnResume();
        }

        protected override async void CleanUp()
        {
            Debug.WriteLine("App On CleanUp");
            base.CleanUp();
        }
    }
}