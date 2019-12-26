using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Service.Autofill;
using Java.Lang;
using OmniCore.Model.Interfaces.Services;
using Unity;

namespace OmniCore.Client.Droid.Services
{
    [Service]
    public class DroidCoreService : Service, ICoreServices
    {
        public ICoreApplicationServices ApplicationServices { get; set; }
        public ICoreDataServices DataServices { get; set; }
        public ICoreIntegrationServices IntegrationServices { get; set; }
        public ICoreAutomationServices AutomationServices { get; set; }

        private static bool IsStarted = false;
        private IUnityContainer Container;

        public async Task StartUp()
        {

            var dbPath = Path.Combine(ApplicationServices.DataPath, "oc.db3");
            await DataServices.RepositoryService.Initialize(dbPath, CancellationToken.None);
        }

        public async Task ShutDown()
        {
            await DataServices.RepositoryService.Shutdown(CancellationToken.None);
        }


        public DroidCoreServiceBinder Binder { get; private set; }
     
        public override IBinder OnBind(Intent intent)
        {
            Binder = new DroidCoreServiceBinder(this);
            if (!IsStarted)
            {
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                    StartForegroundService(intent);
                else
                    StartService(intent);
            }
            return Binder;
        }

        public override void OnCreate()
        {
            Container = Initializer.SetupDependencies();

            ApplicationServices = Container.Resolve<ICoreApplicationServices>();
            DataServices = Container.Resolve<ICoreDataServices>();
            IntegrationServices = Container.Resolve<ICoreIntegrationServices>();
            AutomationServices = Container.Resolve<ICoreAutomationServices>();

            Container.RegisterInstance<ICoreServices>(this);
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (!IsStarted)
            {
                CreateNotification();
                IsStarted = true;
            }

            return StartCommandResult.Sticky;
        }

        public override bool OnUnbind(Intent intent)
        {
            return base.OnUnbind(intent);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void CreateNotification()
        {
            var notificationManager = (NotificationManager) GetSystemService(NotificationService);

            var notificationBuilder = new Notification.Builder(this)
                .SetSmallIcon(Resource.Drawable.ic_pod)
                .SetContentTitle("OmniCore")
                .SetContentText("Service is running");

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                var channelId = "OmniCoreGeneralNotifications";
                var channel = new NotificationChannel(channelId, "Service Notifications", NotificationImportance.Default)
                {
                    Description = "General service notifications."
                };

                notificationManager.CreateNotificationChannel(channel);
                notificationBuilder.SetChannelId(channelId);
            }
            var notification = notificationBuilder.Build();
            notificationManager.Notify(1, notification);
            StartForeground(1, notification);
        }
    }
}