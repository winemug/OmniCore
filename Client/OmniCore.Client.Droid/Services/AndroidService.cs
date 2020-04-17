using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Push;
using OmniCore.Client.Droid.Platform;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client.Droid.Services
{
    [Service(Exported = false, Enabled = true, DirectBootAware = true, Name = "net.balya.OmniCore.Api",
        Icon = "@mipmap/ic_launcher")]
    public class AndroidService : Service, IServiceFunctions
    {
        private bool AndroidServiceStarted;

        private Dictionary<NotificationCategory, NotificationChannel> NotificationChannelDictionary;

        private IContainer<IServiceInstance> Container;
        private IApi Api;
        private const int ServiceNotificationId = 34;

        public ILogger Logger { get; } = new Logger();

        public override IBinder OnBind(Intent intent)
        {
            if (!AndroidServiceStarted)
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    StartForegroundService(intent);
                else
                    StartService(intent);
            }

            return new AndroidServiceBinder(Api);
        }

        public override void OnCreate()
        {
            InitializeNotifications();
            Container = Initializer.AndroidServiceContainer(this);
            Api = Container.Get<IApi>();
            
            if (!AppCenter.Configured)
                Push.PushNotificationReceived += (sender, e) =>
                {
                    // Add the notification message and title to the message
                    var summary = "Push notification received:" +
                                  $"\n\tNotification title: {e.Title}" +
                                  $"\n\tMessage: {e.Message}";

                    // If there is custom data associated with the notification,
                    // print the entries
                    if (e.CustomData != null)
                    {
                        summary += "\n\tCustom data:\n";
                        foreach (var key in e.CustomData.Keys) summary += $"\t\t{key} : {e.CustomData[key]}\n";
                    }

                    // Send the notification summary to debug output
                    Logger.Debug(summary);
                };

            AppCenter.Start("android=51067176-2950-4b0e-9230-1998460d7981;", typeof(Analytics), typeof(Crashes),
                typeof(Push));
            base.OnCreate();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (!AndroidServiceStarted)
            {
                var serviceNotification = SetNotification(
                    ServiceNotificationId,
                        "OmniCore Android Service",
                        "OmniCore is starting...",
                        NotificationCategory.ApplicationInformation);

                StartForeground(ServiceNotificationId, serviceNotification);
                AndroidServiceStarted = true;

                Task.Run(async () => await Api.StartServices(CancellationToken.None));

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
            var t = Task.Run(async () => await Api.StopServices(CancellationToken.None));
            t.Wait();
            if (!t.IsCompletedSuccessfully)
                //TODO: log
                throw new OmniCoreWorkflowException(FailureType.ServiceStopFailure, null, t.Exception);
            ClearNotifications();
            DeinitializeNotifications();
            base.OnDestroy();
        }

        private void InitializeNotifications()
        {
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
            var notificationManager = (NotificationManager) GetSystemService(NotificationService);
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
                notificationChannel.Dispose();
        }
       
        private Notification SetNotification(int id, string title, string message, NotificationCategory category)
        {
            var notificationManager = (NotificationManager)
                GetSystemService(Context.NotificationService);
#pragma warning disable CS0618 // 'Notification.Builder.Builder(Context)' is obsolete: 'deprecated'

            var notificationBuilder = new Android.App.Notification.Builder(this)
#pragma warning restore CS0618 // 'Notification.Builder.Builder(Context)' is obsolete: 'deprecated'
                .SetSmallIcon(Resource.Drawable.ic_stat_pod);
            if (!string.IsNullOrEmpty(title))
                notificationBuilder.SetContentTitle(title);
            if (!string.IsNullOrEmpty(message))
            {
                notificationBuilder.SetStyle(new Android.App.Notification.BigTextStyle());
                notificationBuilder.SetContentText(message);
            }

            notificationBuilder.SetChannelId(category.ToString("G"));
            notificationBuilder.SetOnlyAlertOnce(true);
            // notificationBuilder.SetAutoCancel(AutoDismiss);
            //if (Timeout.HasValue)
            //    notificationBuilder.SetTimeoutAfter((long) Timeout.Value.TotalMilliseconds);

            var notification = notificationBuilder.Build();
            notificationManager.Notify(id, notification);
            return notification;
        }
        
        public void ClearNotifications()
        {
            var notificationManager = (NotificationManager)
                GetSystemService(NotificationService);
            notificationManager.CancelAll();
        }

    }
}