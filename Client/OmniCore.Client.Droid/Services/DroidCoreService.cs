using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using OmniCore.Model.Interfaces.Services;
using Unity;

namespace OmniCore.Client.Droid.Services
{
    [Service]
    public class DroidCoreService : Service, ICoreServices
    {
        [Dependency]
        public ICoreApplicationServices ApplicationServices { get; set; }
        [Dependency]
        public ICoreDataServices DataServices { get; set; }
        [Dependency]
        public ICoreIntegrationServices IntegrationServices { get; set; }
        [Dependency]
        public ICoreAutomationServices AutomationServices { get; set; }

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
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                StartForegroundService(intent);
            }
            else
            {
                StartService(intent);
            }

            return Binder;
        }

        public override void OnCreate()
        {
            base.OnCreate();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var notification = new Notification.Builder(this)
                .SetContentTitle("OmniCore")
                .SetContentText("Service is running")
                .Build();
        
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Notify(1, notification);

            StartForeground(1, notification);

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
    }
}