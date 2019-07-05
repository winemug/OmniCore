using OmniCore.Mobile.Base;
using OmniCore.Mobile.Services;
using OmniCore.Model.Eros;
using OmniCore.Model.Interfaces;
using OmniCore.Radio.RileyLink;
using System.Threading;
using Xamarin.Forms;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

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

        public void GoBack()
        {
            MainPage.SendBackButtonPressed();
        }

        protected override void OnStart()
        {
            AppCenter.Start("android=51067176-2950-4b0e-9230-1998460d7981;", typeof(Analytics), typeof(Crashes));
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
