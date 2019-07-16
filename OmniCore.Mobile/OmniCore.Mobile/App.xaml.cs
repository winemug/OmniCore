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
using OmniCore.Model.Exceptions;
using OmniCore.Model.Utilities;

namespace OmniCore.Mobile
{
    public partial class App : Application
    {
        public static App Instance => Application.Current as App;
        public IPodProvider PodProvider { get; private set; }
        public IMessageExchangeProvider ExchangeProvider { get; private set; }

        public App()
        {
            OmniCoreServices.UiSyncContext = SynchronizationContext.Current;
            PodProvider = new ErosPodProvider();
            PodProvider.Initialize().ExecuteSynchronously();
            ExchangeProvider = new RileyLinkMessageExchangeProvider();
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
            Crashes.ShouldProcessErrorReport = report => !(report.Exception is OmniCoreException);
            OmniCoreServices.Logger.Debug("OmniCore App OnStart called");
        }

        protected override void OnSleep()
        {
            MessagingCenter.Send(this, MessagingConstants.AppSleeping);
            var repo = ErosRepository.GetInstance().ExecuteSynchronously();
            repo.Dispose();
            OmniCoreServices.Logger.Debug("OmniCore App OnSleep called");
        }

        protected override void OnResume()
        {
            var repo = ErosRepository.GetInstance().ExecuteSynchronously();
            repo.Initialize().ExecuteSynchronously();
            OmniCoreServices.AppState.TryRemove(AppStateConstants.ActiveConversation);
            OmniCoreServices.Logger.Debug("OmniCore App OnResume called");
            MessagingCenter.Send(this, MessagingConstants.AppResuming);
        }
    }
}
