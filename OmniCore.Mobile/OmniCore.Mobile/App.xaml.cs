using OmniCore.Mobile.Services;
using OmniCore.Mobile.Views;
using System;
using System.ComponentModel;
using System.Diagnostics;
using OmniCore.Mobile.ViewModels;
using OmniCore.Services;
using OmniCore.Services.Interfaces;
using Unity;
using Unity.Lifetime;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Mobile
{
    public partial class App : Application
    {
        private static IUnityContainer _container;
        public static IUnityContainer Container
        {
            get
            {
                if (_container == null)
                    _container = new UnityContainer();
                return _container;
            }
        }

        private NavigationService _navigationService;
        
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            InitializeComponent();
            var shell = new AppShell();
            _navigationService = new NavigationService(shell);
            RegisterServices();
            MainPage = shell;
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"Unhandled exception: {e.ExceptionObject}");
        }

        private void RegisterServices()
        {
            Container.RegisterInstance(_navigationService);
            Container.RegisterType<BgcService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ConfigurationStore>(new ContainerControlledLifetimeManager());
            Container.RegisterType<DataStore>(new ContainerControlledLifetimeManager());
            Container.RegisterType<XDripWebServiceClient>(new ContainerControlledLifetimeManager());
            Container.RegisterType<SyncClient>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ApiClient>(new ContainerControlledLifetimeManager());
            
            _navigationService.Register<StartPage, StartViewModel>();
            _navigationService.Register<PlatformConfigurationPage, PlatformConfigurationViewModel>();
            _navigationService.Register<AccountLoginPage, AccountLoginViewModel>();
            _navigationService.Register<ClientRegistrationPage, ClientRegistrationViewModel>();
            
            _navigationService.Register<TwoFactorAuthenticationPage>();
        }

        protected override async void OnStart()
        {
            Debug.WriteLine($"App On Start");
            base.OnStart();
        }

        protected override async void OnSleep()
        {
            Debug.WriteLine($"App On Sleep");
            await _navigationService.OnSleepAsync();
            base.OnSleep();
        }

        protected override async void OnResume()
        {
            Debug.WriteLine($"App On Resume");
            await _navigationService.OnResumeAsync();
            base.OnResume();
        }

        protected override async void CleanUp()
        {
            Debug.WriteLine($"App On CleanUp");
            base.CleanUp();
        }
    }
}
