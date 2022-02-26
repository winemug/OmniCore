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
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            RegisterServices();
            InitializeComponent();
            MainPage = new AppShell();
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"Unhandled exception: {e.ExceptionObject}");
        }

        private void RegisterServices()
        {
            Container.RegisterType<ViewModelBindingRegistry>(new ContainerControlledLifetimeManager());
            Container.RegisterType<BgcService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ConfigurationStore>(new ContainerControlledLifetimeManager());
            Container.RegisterType<DataStore>(new ContainerControlledLifetimeManager());
            Container.RegisterType<XDripWebServiceClient>(new ContainerControlledLifetimeManager());
            Container.RegisterType<SyncClient>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ApiClient>(new ContainerControlledLifetimeManager());
            
            var vbr = Container.Resolve<ViewModelBindingRegistry>();
            vbr.RegisterModelBinding<StartPage, StartViewModel>();
            vbr.RegisterModelBinding<PlatformConfigurationPage, PlatformConfigurationViewModel>();
            vbr.RegisterModelBinding<AccountLoginPage, AccountLoginViewModel>();
        }

        protected override async void OnStart()
        {
            Debug.WriteLine($"App On Start");
            base.OnStart();
        }

        protected override async void OnSleep()
        {
            Debug.WriteLine($"App On Sleep");
            base.OnSleep();
        }

        protected override async void OnResume()
        {
            Debug.WriteLine($"App On Resume");
            base.OnResume();
        }

        protected override async void CleanUp()
        {
            Debug.WriteLine($"App On CleanUp");
            base.CleanUp();
        }
    }
}
