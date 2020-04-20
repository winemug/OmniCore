using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client.Droid.Services
{
    [Service(Exported = false, Enabled = true, DirectBootAware = false, Name = "net.balya.OmniCore.ForegroundTaskService",
        Icon = "@mipmap/ic_launcher")]
    public class ForegroundTaskService : Service, IForegroundTaskService
    {
        private bool ForegroundTaskServiceStarted = false;
        private const string NotificationChannelId = "OmnicoreForegroundTask";
        private const string NotificationChannelName = "Omnicore task notification";
        private const int ForegroundTaskNotificationId = 32;
        private NotificationChannel ForegroundNotificationChannel;
        private Notification ForegroundNotification;
        private NotificationManager NotificationManager;
        private ConcurrentBag<IForegroundTask> ForegroundTasks;
        
        public override IBinder OnBind(Intent intent)
        {
            if (!ForegroundTaskServiceStarted)
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    StartForegroundService(intent);
                else
                    StartService(intent);
            }
            return new ForegroundTaskServiceBinder(this);
        }

        public override bool OnUnbind(Intent intent)
        {
            NotificationManager.CancelAll();
            return base.OnUnbind(intent);
        }
        public override void OnCreate()
        {
            base.OnCreate();
            NotificationManager = (NotificationManager) GetSystemService(NotificationService);
            ForegroundNotificationChannel = new NotificationChannel(NotificationChannelId, NotificationChannelName, NotificationImportance.Default)
            {
                Description = "Displays information about the active task"
            };
            NotificationManager.CreateNotificationChannel(ForegroundNotificationChannel);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            NotificationManager.DeleteNotificationChannel(NotificationChannelId);
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            lock (this)
            {
                if (!ForegroundTaskServiceStarted)
                {
                    var notificationBuilder = new Android.App.Notification.Builder(this)
                        .SetSmallIcon(Resource.Drawable.ic_stat_pod);

                    notificationBuilder.SetContentTitle("OmniCore is running");

                    // notificationBuilder.SetStyle(new Android.App.Notification.BigTextStyle());
                    // notificationBuilder.SetContentText("Notification goes here");
                    notificationBuilder.SetChannelId(NotificationChannelId);
                    notificationBuilder.SetOnlyAlertOnce(true);

                    ForegroundNotification = notificationBuilder.Build();
                    NotificationManager.Notify(ForegroundTaskNotificationId, ForegroundNotification);

                    StartForeground(ForegroundTaskNotificationId, ForegroundNotification);
                    ForegroundTaskServiceStarted = true;
                }
            }
            return StartCommandResult.NotSticky;
        }
        public async Task ExecuteTask(Action<Task> foregroundTask, CancellationToken cancellationToken)
        {
        }
    }
}