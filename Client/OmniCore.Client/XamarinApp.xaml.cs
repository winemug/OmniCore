using System.Threading;
using Xamarin.Forms;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Main;
using OmniCore.Model.Interfaces.Platform;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using OmniCore.Model.Interfaces.Services;
using Unity;

namespace OmniCore.Client
{
    public partial class XamarinApp : Application, IUserInterface
    {
        public SynchronizationContext SynchronizationContext { get; }
        public Task ShutDown()
        {
            throw new NotImplementedException();
        }

        public IObservable<IUserInterface> WhenStarting()
        {
            return SubjectStarting.AsObservable();
        }

        public IObservable<IUserInterface> WhenHibernating()
        {
            return SubjectHibernating.AsObservable();
        }

        public IObservable<IUserInterface> WhenResuming()
        {
            return SubjectResuming.AsObservable();
        }

        private readonly Subject<IUserInterface> SubjectStarting;
        private readonly Subject<IUserInterface> SubjectHibernating;
        private readonly Subject<IUserInterface> SubjectResuming;
        
        private readonly ICoreServices CoreServices;
        private ICoreApplicationLogger ApplicationLogger => CoreServices.ApplicationLogger;
            
        public XamarinApp(ICoreServicesProvider coreServicesProvider, IUnityContainer container)
        {
            SubjectStarting = new Subject<IUserInterface>();
            SubjectHibernating = new Subject<IUserInterface>();
            SubjectResuming = new Subject<IUserInterface>();
            
            CoreServices = coreServicesProvider.LocalServices;
            SynchronizationContext = SynchronizationContext.Current;

            InitializeComponent();

            MainPage = GetMainPage(container);
            ApplicationLogger.Information("OmniCore App initialized");
        }

        protected override async void OnStart()
        {
            AppCenter.Start("android=51067176-2950-4b0e-9230-1998460d7981;", typeof(Analytics), typeof(Crashes));
            //Crashes.ShouldProcessErrorReport = report => !(report.Exception is OmniCoreException);
            ApplicationLogger.Debug("OmniCore App OnStart called");
            await EnsurePermissions();
            SubjectStarting.OnNext(this);
            SubjectStarting.OnCompleted();
        }

        protected override void OnSleep()
        {
            ApplicationLogger.Debug("OmniCore App OnSleep called");
            SubjectHibernating.OnNext(this);
        }

        protected override void OnResume()
        {
            ApplicationLogger.Debug("OmniCore App OnResume called");
            SubjectResuming.OnNext(this);
        }

        private async Task EnsurePermissions()
        {
            try
            {
                if (
                    !await CheckAndRequestPermission(
                    Permission.LocationAlways,
                    "Please grant the location permission to this application in order to be able to connect to bluetooth devices.")
                    ||
                    !await CheckAndRequestPermission(
                        Permission.Storage,
                        "Please grant the storage permission to this application in order to be able to import and export files.")
                )
                {
                    await MainPage.DisplayAlert("Missing Permissions", "OmniCore cannot run without the necessary permissions.", "OK");
                    await ShutDown();
                }

            }
            catch (Exception e)
            {
                await MainPage.DisplayAlert("Missing Permissions", "Error while querying / acquiring permissions", "OK");
                Crashes.TrackError(e);
                await ShutDown();
            }
        }

        private async Task<bool> CheckAndRequestPermission(Permission permission, string requestMessage)
        {
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(permission);
            if (status != PermissionStatus.Granted)
            {
                await MainPage.DisplayAlert("Missing Permissions", requestMessage, "OK");
                var request = await CrossPermissions.Current.RequestPermissionsAsync(permission);
                return request[permission] == PermissionStatus.Granted;
            }
            return true;
        }

        private Page GetMainPage(IUnityContainer container)
        {
            return container.Resolve<ShellView>();
        }

    }
}
