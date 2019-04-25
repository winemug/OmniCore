using nexus.protocols.ble;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Mobile
{
    public partial class App : Application
    {
        public static App Instance => Application.Current as App;

        public IBluetoothLowEnergyAdapter BleAdapter { get; private set; }

        public App(IBluetoothLowEnergyAdapter ble)
        {
            this.BleAdapter = ble;
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
