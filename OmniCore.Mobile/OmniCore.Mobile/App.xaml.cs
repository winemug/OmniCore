using OmniCore.Data;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Mobile
{
    public partial class App : Application
    {
        public static App Instance => Application.Current as App;

        public App()
        {
            InitializeComponent();
            MainPage = new Views.OmniCoreMain();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            DataStore.Instance.Initialize();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
