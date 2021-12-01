using OmniCore.Mobile.Services;
using OmniCore.Mobile.Views;
using System;
using OmniCore.Services.Data;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Mobile
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
            DependencyService.Register<MockDataStore>();
            DependencyService.RegisterSingleton(new DataStore());
           
            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            await DependencyService.Get<DataStore>().Initialize();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
