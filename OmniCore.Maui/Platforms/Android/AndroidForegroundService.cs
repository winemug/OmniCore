using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using static Android.App.Notification;

namespace OmniCore.Maui
{
    [Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeSystemExempted)]
    public class AndroidForegroundService : Service
    {
        private bool _isStarted;
        private Task? _startTask;
        public override IBinder OnBind(Intent intent)
        {
            return new Binder();
        }

        private Notification GetForegroundNotification()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var notificationManager = GetSystemService(NotificationService) as NotificationManager;
                var channel = new NotificationChannel("background", "Background notification",
                    NotificationImportance.Low);
                notificationManager.CreateNotificationChannel(channel);
            }


            // this.PackageManager.GetLaunchIntentForPackage()
            // var launchIntent = new Intent(this, typeof(MainActivity));
            // showAppIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTask);
            // var showAppPendingIntent = PendingIntent.GetActivity(this, 0, showAppIntent, PendingIntentFlags.UpdateCurrent);

            return new Builder(this, "background")
                .SetContentTitle("OmniCore")
                .SetContentText("OmniCore is running in the background.")
                // .SetContentIntent(showAppPendingIntent)
                .SetOngoing(true)
                .Build();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            switch (intent.Action)
            {
                case "start":
                    if (!_isStarted)
                    {
                        var notification = GetForegroundNotification();
                        StartForeground(100, notification, ForegroundService.TypeSystemExempted);

                        // if (_startTask == null)
                        // {
                        //     var coreService = IPlatformApplication.Current!.Services.GetRequiredService<ICoreService>();
                        //     _startTask = coreService.Start();
                        // }
                        _isStarted = true;
                    }
                    return StartCommandResult.Sticky;
                case "stop":
                    if (_isStarted)
                    {
                        _startTask?.Wait();
                        _startTask = null;
                        // var coreService = IPlatformApplication.Current!.Services.GetRequiredService<ICoreService>();
                        // coreService.Stop().Wait();
                        StopForeground(StopForegroundFlags.Remove);
                        StopSelf();
                        _isStarted = false;
                    }
                    return StartCommandResult.NotSticky;
            }

            return StartCommandResult.ContinuationMask;
        }
    }
}