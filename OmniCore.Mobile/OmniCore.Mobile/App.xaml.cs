using OmniCore.Py;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Mobile
{
    public partial class App : Application
    {
        public static App Instance => Application.Current as App;

        public OmnipyStub Py { get; private set; }

        public App()
        {
            this.Py = new OmnipyStub();
            InitializeComponent();
            MainPage = new Views.MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
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
