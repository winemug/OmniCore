using OmniCore.Client.Services;
using OmniCore.Eros;
using OmniCore.Model.Interfaces;
using System.Threading;
using Xamarin.Forms;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Utilities;
using Unity;
using OmniCore.Client.Interfaces;
using OmniCore.Client.Views;
using OmniCore.Client.Constants;

namespace OmniCore.Client
{
    public partial class App : Application
    {
        public static App Instance => Application.Current as App;
        public IPodProvider PodProvider { get; }
        public IOmniCoreLogger Logger { get; }
        public IOmniCoreApplication OmniCoreApplication { get; }

        public SynchronizationContext UiSyncContext;

        public App(IUnityContainer container)
        {
            Initializer.RegisterTypes(container);

            PodProvider = container.Resolve<IPodProvider>();
            Logger = container.Resolve<IOmniCoreLogger>();
            OmniCoreApplication = container.Resolve<IOmniCoreApplication>();

            InitializeComponent();

            UiSyncContext = SynchronizationContext.Current;
            MainPage = new MainPage();

            //OmniCoreServices.Publisher.Subscribe(new RemoteRequestHandler());
            Logger.Information("OmniCore App initialized");
        }

        public void GoBack()
        {
            MainPage.SendBackButtonPressed();
        }

        protected override void OnStart()
        {
            AppCenter.Start("android=51067176-2950-4b0e-9230-1998460d7981;", typeof(Analytics), typeof(Crashes));
            Crashes.ShouldProcessErrorReport = report => !(report.Exception is OmniCoreException);
            Logger.Debug("OmniCore App OnStart called");
        }

        protected override void OnSleep()
        {
            MessagingCenter.Send(this, MessagingConstants.AppSleeping);
            Logger.Debug("OmniCore App OnSleep called");
        }

        protected override void OnResume()
        {
            OmniCoreApplication.State.TryRemove(AppStateConstants.ActiveConversation);
            Logger.Debug("OmniCore App OnResume called");
            MessagingCenter.Send(this, MessagingConstants.AppResuming);
        }
    }
}
