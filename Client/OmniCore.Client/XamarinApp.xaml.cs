using System.Threading;
using Xamarin.Forms;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.IO;
using System.Threading.Tasks;
using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Main;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Services;
using Unity;

namespace OmniCore.Client
{
    public partial class XamarinApp : Application, IUserInterface
    {
        public SynchronizationContext SynchronizationContext { get; }

        private readonly ICoreServices CoreServices;
        private IApplicationLogger Logger => CoreServices.ApplicationService.Logger;
            
        public XamarinApp(ICoreServicesProvider coreServicesProvider, IUnityContainer container)
        {
            CoreServices = coreServicesProvider.LocalServices;
            SynchronizationContext = SynchronizationContext.Current;

            InitializeComponent();

            MainPage = GetMainPage(container);
            Logger.Information("OmniCore App initialized");
        }

        protected async override void OnStart()
        {
            AppCenter.Start("android=51067176-2950-4b0e-9230-1998460d7981;", typeof(Analytics), typeof(Crashes));
            //Crashes.ShouldProcessErrorReport = report => !(report.Exception is OmniCoreException);
            Logger.Debug("OmniCore App OnStart called");
            await EnsurePermissions();
            var dbPath = Path.Combine(CoreServices.ApplicationService.DataPath, "omnicore.db3");
            await CoreServices.RepositoryService.Initialize(dbPath, CancellationToken.None);
        }

        protected async override void OnSleep()
        {
            Logger.Debug("OmniCore App OnSleep called");
        }

        protected async override void OnResume()
        {
            Logger.Debug("OmniCore App OnResume called");
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
                    CoreServices.ApplicationService.Shutdown();
                }

            }
            catch (Exception e)
            {
                await MainPage.DisplayAlert("Missing Permissions", "Error while querying / acquiring permissions", "OK");
                Crashes.TrackError(e);
                CoreServices.ApplicationService.Shutdown();
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
