using OmniCore.Mobile.Services;
using OmniCore.Mobile.Views;
using System;
using System.ComponentModel;
using OmniCore.Services;
using OmniCore.Services.Interfaces;
using Unity;
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
            InitializeComponent();
            RegisterServices();
            MainPage = new AppShell();
        }

        private void RegisterServices()
        {
            Container.RegisterInstance(new ConfigurationStore(), InstanceLifetime.Singleton);
            Container.RegisterInstance(new DataStore(), InstanceLifetime.Singleton);
            Container.RegisterInstance(new XDripWebServiceClient(), InstanceLifetime.Singleton);
            Container.RegisterInstance(new SyncClient(), InstanceLifetime.Singleton);
            Container.RegisterInstance(new ApiClient(), InstanceLifetime.Singleton);
        }

        protected override async void OnStart()
        {
            await Shell.Current.GoToAsync("//StartPage");
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
