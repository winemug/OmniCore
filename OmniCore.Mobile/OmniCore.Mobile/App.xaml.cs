using OmniCore.Mobile.Base;
using OmniCore.Mobile.Base.Interfaces;
using OmniCore.Mobile.Services;
using OmniCore.Model.Eros;
using OmniCore.Model.Interfaces;
using OmniCore.Radio.RileyLink;
using Plugin.Permissions;
using System;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Mobile
{
    public partial class App : Application
    {
        public static App Instance => Application.Current as App;
        public IPodProvider PodProvider { get; private set; }

        public App()
        {
            OmniCoreServices.UiSyncContext = SynchronizationContext.Current;
            PodProvider = new ErosPodProvider(new RileyLinkMessageExchangeProvider());
            OmniCoreServices.Publisher.Subscribe(new RemoteRequestHandler());

            InitializeComponent();
            MainPage = new Views.MainPage();
            OmniCoreServices.Logger.Information("OmniCore App initialized");
        }

        protected override void OnStart()
        {
            OmniCoreServices.Logger.Debug("OmniCore App OnStart called");
        }

        protected override void OnSleep()
        {
            OmniCoreServices.Logger.Debug("OmniCore App OnSleep called");
        }

        protected override void OnResume()
        {
            OmniCoreServices.Logger.Debug("OmniCore App OnResume called");
        }
    }
}
