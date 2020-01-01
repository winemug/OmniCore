using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Acr.Logging;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Service.Autofill;
using Java.Lang;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using OmniCore.Eros;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Radios.RileyLink;
using OmniCore.Repository.Sqlite;
using OmniCore.Services;
using Unity;

namespace OmniCore.Client.Droid.Services
{
    [Service]
    public class CoreBootstrapper : Service, ICoreBootstrapper
    {
        private bool AndroidServiceStarted = false;

        public CoreServiceBinder Binder { get; private set; }

        public ICoreContainer Container { get; private set; }

        public override IBinder OnBind(Intent intent)
        {
            Binder = new CoreServiceBinder(this);
            if (!AndroidServiceStarted)
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
            Container = new OmniCoreContainer()
                .Existing<ICoreBootstrapper>(this)
                .WithOmnipodEros()
                .WithRileyLinkRadio()
                .WithAapsIntegration()
#if EMULATOR
                .WithBleSimulator()
#else
                .WithCrossBleAdapter()
#endif
                .WithSqliteRepositories()
                .WithXamarinForms()
                .OnAndroidPlatform();

            LoggingService = Container.Get<ICoreLoggingService>();
            ApplicationService = Container.Get<ICoreApplicationService>();
            RepositoryService = Container.Get<IRepositoryService>();
            RadioService = Container.Get<IRadioService>();
            PodService = Container.Get<IPodService>();
            IntegrationService = Container.Get<ICoreIntegrationService>();

            StartServices(CancellationToken.None);
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (!AndroidServiceStarted)
            {
                CreateNotification();
                AndroidServiceStarted = true;
            }

            return StartCommandResult.Sticky;
        }

        public override bool OnUnbind(Intent intent)
        {
            return base.OnUnbind(intent);
        }

        public override void OnDestroy()
        {
            StopServices(CancellationToken.None);
            base.OnDestroy();
        }

        private void CreateNotification()
        {
            var notificationManager = (NotificationManager) GetSystemService(NotificationService);

            var notificationBuilder = new Notification.Builder(this)
                .SetSmallIcon(Resource.Drawable.ic_stat_pod)
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

        public async Task StartServices(CancellationToken cancellationToken)
        {
            await LoggingService.StartService(cancellationToken);
            await ApplicationService.StartService(cancellationToken);
            await RepositoryService.StartService(cancellationToken);
            await RadioService.StartService(cancellationToken);
            await PodService.StartService(cancellationToken);
            await IntegrationService.StartService(cancellationToken);
        }

        public async Task StopServices(CancellationToken cancellationToken)
        {
            await IntegrationService.StopService(cancellationToken);
            await PodService.StopService(cancellationToken);
            await RadioService.StopService(cancellationToken);
            await RepositoryService.StopService(cancellationToken);
            await ApplicationService.StopService(cancellationToken);
            await LoggingService.StopService(cancellationToken);
        }

        public ICoreLoggingService LoggingService { get; private set; }
        public ICoreApplicationService ApplicationService { get; private set; }
        public IRepositoryService RepositoryService { get; private set; }
        public IRadioService RadioService { get; private set; }
        public IPodService PodService { get; private set; }
        public ICoreIntegrationService IntegrationService { get; private set; }
    }
}