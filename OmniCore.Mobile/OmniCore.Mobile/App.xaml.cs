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
            InitializeComponent();
            MainPage = new Views.MainPage();
            OmniCoreServices.Logger.Information("OmniCore App initialized");
        }

        public void GoBack()
        {
            MainPage.SendBackButtonPressed();
        }

        protected override async void OnStart()
        {
            AppCenter.Start("android=51067176-2950-4b0e-9230-1998460d7981;", typeof(Analytics), typeof(Crashes));
            OmniCoreServices.Logger.Debug("OmniCore App OnStart called");
            await PodProvider.Initialize();
            OmniCoreServices.Publisher.Subscribe(new RemoteRequestHandler());
        }

        protected override void OnSleep()
        {
            ErosRepository.Instance.Dispose();
            OmniCoreServices.Logger.Debug("OmniCore App OnSleep called");
        }

        protected override async void OnResume()
        {
            await ErosRepository.Instance.Initialize();
            OmniCoreServices.Logger.Debug("OmniCore App OnResume called");
        }
    }
}
