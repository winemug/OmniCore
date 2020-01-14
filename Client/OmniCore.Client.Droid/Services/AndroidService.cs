using System;
using System.Collections.Generic;
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
using OmniCore.Client.Droid;
using OmniCore.Client.Droid.Platform;
using OmniCore.Client.Droid.Services;
using OmniCore.Eros;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Server;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Radios.RileyLink;
using OmniCore.Repository.Sqlite;
using OmniCore.Services;
using Unity;
using Notification = Android.App.Notification;

namespace OmniCore.Client.Droid.Services
{
    [Service(Exported = false, Enabled = true, DirectBootAware = true, Name = "net.balya.OmniCore.ServiceApi", Icon="@mipmap/ic_launcher")]
    public class AndroidService : Service, ICoreServiceApi, ICoreNotificationFunctions
    {
        private bool AndroidServiceStarted = false;

        public ICoreContainer<IServerResolvable> ServerContainer { get; private set; }
        public ICoreLoggingFunctions LoggingFunctions => ServerContainer.Get<ICoreLoggingFunctions>();
        public ICoreApplicationFunctions ApplicationFunctions => ServerContainer.Get<ICoreApplicationFunctions>();
        public IRepositoryService RepositoryService => ServerContainer.Get<IRepositoryService>();
        public IRadioService RadioService => ServerContainer.Get<IRadioService>();
        public IPodService PodService => ServerContainer.Get<IPodService>();
        public ICoreNotificationFunctions NotificationFunctions => ServerContainer.Get<ICoreNotificationFunctions>();
        public ICoreIntegrationService IntegrationService => ServerContainer.Get<ICoreIntegrationService>();

        private ISubject<ICoreServiceApi> UnexpectedStopRequestSubject;
        private CoreNotification ServiceNotification; 

        public override IBinder OnBind(Intent intent)
        {
            if (!AndroidServiceStarted)
            {
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                    StartForegroundService(intent);
                else
                    StartService(intent);
            }
            return new AndroidServiceBinder(this);
        }

        public override void OnCreate()
        {
            UnexpectedStopRequestSubject = new Subject<ICoreServiceApi>();
            ServerContainer = Initializer.AndroidServiceContainer(this, this);
            base.OnCreate();
            InitializeNotifications();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (!AndroidServiceStarted)
            {
                ServiceNotification = NotificationFunctions.CreateNotification(NotificationCategory.ApplicationInformation,
                    "Service running in background", null)
                    as CoreNotification;
                this.StartForeground(ServiceNotification.Id, ServiceNotification.NativeNotification);
                AndroidServiceStarted = true;

                var t = Task.Run(async () => await StartServices(CancellationToken.None));
                t.Wait();
                if (!t.IsCompletedSuccessfully)
                {
                    //TODO: log
                    throw new OmniCoreWorkflowException(FailureType.ServiceStartupFailure, null, t.Exception);
                }
            }

            return StartCommandResult.Sticky;
        }

        public override bool OnUnbind(Intent intent)
        {
            return base.OnUnbind(intent);
        }

        public override void OnDestroy()
        {
            UnexpectedStopRequested();
            var t = Task.Run(async () => await StopServices(CancellationToken.None));
            t.Wait();
            if (!t.IsCompletedSuccessfully)
            {
                //TODO: log
                throw new OmniCoreWorkflowException(FailureType.ServiceStopFailure, null, t.Exception);
            }
            ServiceNotification.Dismiss();
            base.OnDestroy();
        }
        public async Task StartServices(CancellationToken cancellationToken)
        {
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
        }
        
        public void UnexpectedStopRequested()
        {
            UnexpectedStopRequestSubject.OnNext(this);
        }

        private Dictionary<NotificationCategory,NotificationChannel> NotificationChannelDictionary;
        private int NotificationIdCounter;

        private void InitializeNotifications()
        {
            NotificationIdCounter = 0;
            NotificationChannelDictionary = new Dictionary<NotificationCategory, NotificationChannel>();
            CreateNotificationChannel(NotificationCategory.ApplicationInformation,
                "General Information", "General notifications.",
                NotificationImportance.Low);

            CreateNotificationChannel(NotificationCategory.ApplicationWarning,
                "General Warning", "General warning notifications.",
                NotificationImportance.Default);

            CreateNotificationChannel(NotificationCategory.ApplicationImportant,
                "General Important", "Important notifications requiring user attention.",
                NotificationImportance.Default);

            CreateNotificationChannel(NotificationCategory.RadioInformation,
                "Radio Information", "Radio related informational notifications.",
                NotificationImportance.Low);

            CreateNotificationChannel(NotificationCategory.RadioWarning,
                "Radio Warning", "Radio warning notifications.",
                NotificationImportance.Default);

            CreateNotificationChannel(NotificationCategory.RadioImportant,
                "Radio Important", "Important notifications requiring user attention.",
                NotificationImportance.Default);

            CreateNotificationChannel(NotificationCategory.PodInformation,
                "Pod General", "Pod related informational notifications.",
                NotificationImportance.Low);

            CreateNotificationChannel(NotificationCategory.PodWarning,
                "Pod Warning", "Pod related warning notifications.",
                NotificationImportance.Default);

            CreateNotificationChannel(NotificationCategory.PodImportant,
                "Pod Important", "Important notifications requiring user attention.",
                NotificationImportance.Default);

            CreateNotificationChannel(NotificationCategory.PodImmediateAction,
                "Pod Important", "Very important notifications requiring immediate user attention.",
                NotificationImportance.High);
        }

        private void CreateNotificationChannel(NotificationCategory category, string title, string description,
            NotificationImportance importance)
        {
            var notificationManager = (NotificationManager) GetSystemService(Context.NotificationService);
            var channel = new NotificationChannel(category.ToString(), title, importance)
            {
                Description = description
            };
            notificationManager.CreateNotificationChannel(channel);
            NotificationChannelDictionary.Add(category, channel);
        }

        public ICoreNotification CreateNotification(NotificationCategory category,
            string title, string message)
        {
            var notification = ServerContainer.Get<ICoreNotification>() as CoreNotification;
            if (notification == null)
            {
                //TODO: throw?
            }
            var notificationId = Interlocked.Increment(ref NotificationIdCounter);
            notification.CreateInternal(this, notificationId, category, title, message);
            return notification;
        }

        public void ClearNotifications()
        {
            throw new NotImplementedException();
        }

        public IObservable<ICoreNotification> WhenNotificationAdded()
        {
            throw new NotImplementedException();
        }

        public IObservable<ICoreNotification> WhenNotificationDismissed()
        {
            throw new NotImplementedException();
        }
    }
}