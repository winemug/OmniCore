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
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Push;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using OmniCore.Client.Droid;
using OmniCore.Client.Droid.Platform;
using OmniCore.Client.Droid.Services;
using OmniCore.Eros;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Radios.RileyLink;
using OmniCore.Services;
using Unity;
using Enum = Java.Lang.Enum;
using Notification = Android.App.Notification;

namespace OmniCore.Client.Droid.Services
{
    [Service(Exported = false, Enabled = true, DirectBootAware = true, Name = "net.balya.OmniCore.Api", Icon="@mipmap/ic_launcher")]
    public class AndroidService : Service, ICoreApi, ICoreNotificationFunctions
    {
        private bool AndroidServiceStarted = false;

        public ICoreContainer<IServerResolvable> ServerContainer { get; private set; }
        public ICoreLoggingFunctions LoggingFunctions => ServerContainer.Get<ICoreLoggingFunctions>();
        public ICoreApplicationFunctions ApplicationFunctions => ServerContainer.Get<ICoreApplicationFunctions>();
        public ICoreRepositoryService CoreRepositoryService => ServerContainer.Get<ICoreRepositoryService>();
        public ICorePodService CorePodService => ServerContainer.Get<ICorePodService>();
        public ICoreNotificationFunctions NotificationFunctions => ServerContainer.Get<ICoreNotificationFunctions>();
        public ICoreIntegrationService IntegrationService => ServerContainer.Get<ICoreIntegrationService>();
        public ICoreAutomationService AutomationService => ServerContainer.Get<ICoreAutomationService>();
        public ICoreConfigurationService CoreConfigurationService => ServerContainer.Get<ICoreConfigurationService>();

        private ISubject<CoreApiStatus> ApiStatusSubject;
        private ISubject<ICoreApi> UnexpectedStopRequestSubject;
        private CoreNotification ServiceNotification; 

        public override IBinder OnBind(Intent intent)
        {
            if (!AndroidServiceStarted)
            {
                if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.O)
                    StartForegroundService(intent);
                else
                    StartService(intent);
            }
            return new AndroidServiceBinder(this);
        }

        public override void OnCreate()
        {
            ApiStatusSubject = new BehaviorSubject<CoreApiStatus>(CoreApiStatus.Starting);

            if (!AppCenter.Configured)
            {
                Push.PushNotificationReceived += (sender, e) =>
                {
                    // Add the notification message and title to the message
                    var summary =  $"Push notification received:" +
                                   $"\n\tNotification title: {e.Title}" +
                                   $"\n\tMessage: {e.Message}";
    
                    // If there is custom data associated with the notification,
                    // print the entries
                    if (e.CustomData != null)
                    {
                        summary += "\n\tCustom data:\n";
                        foreach (var key in e.CustomData.Keys)
                        {
                            summary += $"\t\t{key} : {e.CustomData[key]}\n";
                        }
                    }

                    // Send the notification summary to debug output
                    System.Diagnostics.Debug.WriteLine(summary);
                };
            }

            AppCenter.Start("android=51067176-2950-4b0e-9230-1998460d7981;", typeof(Analytics), typeof(Crashes), typeof(Push));
            UnexpectedStopRequestSubject = new Subject<ICoreApi>();
            ServerContainer = Initializer.AndroidServiceContainer(this, this);
            base.OnCreate();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (!AndroidServiceStarted)
            {
                ServiceNotification = NotificationFunctions.CreateNotification(NotificationCategory.ApplicationInformation,
                    "OmniCore Android Service","OmniCore is starting...")
                    as CoreNotification;
                this.StartForeground(ServiceNotification.Id, ServiceNotification.NativeNotification);
                AndroidServiceStarted = true;

                Task.Run(async () => await StartServices(CancellationToken.None));

                //TODO:
                //var statusNotifiers = ServerContainer.GetAll<INotifyStatus>();
                //foreach (var statusNotifier in statusNotifiers)
                //{
                //}
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
            if (NotificationsInitialized)
                DeinitializeNotifications();
            base.OnDestroy();
        }

        public IObservable<CoreApiStatus> ApiStatus => ApiStatusSubject.AsObservable();

        public async Task StartServices(CancellationToken cancellationToken)
        {
            await CoreRepositoryService.StartService(cancellationToken);
            await CorePodService.StartService(cancellationToken);
            await AutomationService.StartService(cancellationToken);
            await IntegrationService.StartService(cancellationToken);
            ApiStatusSubject.OnNext(CoreApiStatus.Started);
            ServiceNotification.Update(null, "OmniCore is running in background.");
        }

        public async Task StopServices(CancellationToken cancellationToken)
        {
            await IntegrationService.StopService(cancellationToken);
            await AutomationService.StopService(cancellationToken);
            await CorePodService.StopService(cancellationToken);
            await CoreRepositoryService.StopService(cancellationToken);
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
            var channel = new NotificationChannel(category.ToString("G"), title, importance)
            {
                Description = description
            };
            notificationManager.CreateNotificationChannel(channel);

            NotificationChannelDictionary.Add(category, channel);
        }

        private void DeinitializeNotifications()
        {
            foreach (var notificationChannel in NotificationChannelDictionary.Values)
            {
                notificationChannel.Dispose();
            }
        }

        private bool NotificationsInitialized = false;
        public ICoreNotification CreateNotification(NotificationCategory category, string title, string message,
            TimeSpan? timeout = null, bool autoDismiss = true)
        {
            if (!NotificationsInitialized)
            {
                InitializeNotifications();
                NotificationsInitialized = true;
            }

            var notification = ServerContainer.Get<ICoreNotification>() as CoreNotification;
            if (notification == null)
            {
                //TODO: throw?
            }
            var notificationId = Interlocked.Increment(ref NotificationIdCounter);
            notification.CreateInternal(this, notificationId, category, title, message,
                timeout, autoDismiss);
            return notification;
        }

        public void ClearNotifications()
        {
            var notificationManager = (NotificationManager)
                GetSystemService(Context.NotificationService);
            notificationManager.CancelAll();
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