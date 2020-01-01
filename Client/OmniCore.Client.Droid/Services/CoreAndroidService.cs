using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Acr.Logging;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Service.Autofill;
using Java.Lang;
using Java.Util;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using OmniCore.Eros;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Radios.RileyLink;
using OmniCore.Repository.Sqlite;
using OmniCore.Services;
using Unity;
using Notification = Android.App.Notification;

namespace OmniCore.Client.Droid.Services
{
    [Service]
    public class CoreAndroidService : Service, ICoreServices
    {
        private bool AndroidServiceStarted = false;

        public CoreServiceBinder Binder { get; private set; }

        public ICoreContainer Container { get; private set; }

        private readonly ISubject<ICoreServices> UnexpectedStopRequestSubject =
            new Subject<ICoreServices>();
        public IObservable<ICoreServices> OnUnexpectedStopRequest
            => UnexpectedStopRequestSubject;
        
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
                .Existing<ICoreServices>(this)
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

            StartServices(CancellationToken.None).WaitAndUnwrapException();
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
            UnexpectedStopRequestSubject.OnNext(this);
            StopServices(CancellationToken.None).WaitAndUnwrapException();
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
            
            var previousState = ApplicationService.ReadPreferences(new []
            {
                ("CoreAndroidService_StopRequested_RunningServices", string.Empty),
            })[0];
            
            if (!string.IsNullOrEmpty(previousState.Value))
            {
                //TODO: check states of requests - create notifications
                StoreRunningServicesValue(string.Empty);
            }
        }

        private void StoreRunningServicesValue(string value)
        {
            ApplicationService.StorePreferences(new []
            {
                ("CoreAndroidService_StopRequested_RunningServices", string.Empty),
            });
        }

        public async Task StopServices(CancellationToken cancellationToken)
        {
            StoreRunningServicesValue($"{nameof(LoggingService)},{nameof(ApplicationService)}," +
                                      $"{nameof(RepositoryService)},{nameof(RadioService)}," +
                                      $"{nameof(PodService)},{nameof(IntegrationService)}");
            await IntegrationService.StopService(cancellationToken);
            StoreRunningServicesValue($"{nameof(LoggingService)},{nameof(ApplicationService)}," +
                                      $"{nameof(RepositoryService)},{nameof(RadioService)}," +
                                      $"{nameof(PodService)}");
            await PodService.StopService(cancellationToken);
            StoreRunningServicesValue($"{nameof(LoggingService)},{nameof(ApplicationService)}," +
                                      $"{nameof(RepositoryService)},{nameof(RadioService)}");
            await RadioService.StopService(cancellationToken);
            StoreRunningServicesValue($"{nameof(LoggingService)},{nameof(ApplicationService)}," +
                                      $"{nameof(RepositoryService)}");
            await RepositoryService.StopService(cancellationToken);
            StoreRunningServicesValue($"{nameof(LoggingService)},{nameof(ApplicationService)}");
            await ApplicationService.StopService(cancellationToken);
            StoreRunningServicesValue($"{nameof(LoggingService)}");
            await LoggingService.StopService(cancellationToken);
            StoreRunningServicesValue(string.Empty);
        }

        public ICoreLoggingService LoggingService { get; private set; }
        public ICoreApplicationService ApplicationService { get; private set; }
        public IRepositoryService RepositoryService { get; private set; }
        public IRadioService RadioService { get; private set; }
        public IPodService PodService { get; private set; }
        public ICoreIntegrationService IntegrationService { get; private set; }
    }
}