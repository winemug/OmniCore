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
using System.IO;
using System;
using OmniCore.Repository;
using OmniCore.Client.Views.RadioTesting;
using OmniCore.Client.ViewModels.Test;
using System.Threading.Tasks;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Constants;

namespace OmniCore.Client
{
    public partial class App : Application
    {
        public static App Instance => Application.Current as App;
        public IPodProvider PodProvider { get; }
        public IRadioProvider RileyLinkProvider { get;  }
        public IOmniCoreLogger Logger { get; }
        public IOmniCoreApplication OmniCoreApplication { get; }

        public SynchronizationContext UiSyncContext;

        public App(IUnityContainer container)
        {
            PodProvider = container.Resolve<IPodProvider>();
            RileyLinkProvider = container.Resolve<IRadioProvider>("RileyLinkRadioProvider");
            Logger = container.Resolve<IOmniCoreLogger>();
            OmniCoreApplication = container.Resolve<IOmniCoreApplication>();

            RepositoryProvider.Instance.Init();
            InitializeComponent();

            UiSyncContext = SynchronizationContext.Current;
#if DEBUG
            MainPage = new NavigationPage(new RadiosPage().WithViewModel(new RadioTestingViewModel()));
#else
            MainPage = new MainPage();
#endif

            //OmniCoreServices.Publisher.Subscribe(new RemoteRequestHandler());
            Logger.Information("OmniCore App initialized");
        }

        public void GoBack()
        {
            MainPage.SendBackButtonPressed();
        }

        protected async override void OnStart()
        {
            AppCenter.Start("android=51067176-2950-4b0e-9230-1998460d7981;", typeof(Analytics), typeof(Crashes));
            //Crashes.ShouldProcessErrorReport = report => !(report.Exception is OmniCoreException);
            Logger.Debug("OmniCore App OnStart called");
            await EnsurePermissions();
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

        private async Task EnsurePermissions()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.LocationAlways);
                if (status != PermissionStatus.Granted)
                {
                    await MainPage.DisplayAlert("Missing Permissions", "Please grant the location permission to this application in order to be able to connect to bluetooth devices.", "OK");
                    var request = await CrossPermissions.Current.RequestPermissionsAsync(Permission.LocationAlways);
                    if (request[Permission.LocationAlways] != PermissionStatus.Granted)
                    {
                        await MainPage.DisplayAlert("Missing Permissions", "OmniCore cannot run without the necessary permissions.", "OK");
                        App.Instance.OmniCoreApplication.Exit();
                    }
                }
            }
            catch (Exception e)
            {
                await MainPage.DisplayAlert("Missing Permissions", "Error while querying / acquiring permissions", "OK");
                Crashes.TrackError(e);
                App.Instance.OmniCoreApplication.Exit();
            }
        }

    }
}
