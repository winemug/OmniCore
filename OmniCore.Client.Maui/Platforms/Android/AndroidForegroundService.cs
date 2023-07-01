using Android.App;
using Android.Content;
using Android.OS;
using OmniCore.Common.Core;
using static Android.App.Notification;

namespace OmniCore.Maui
{
    [Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeDataSync |
                                     global::Android.Content.PM.ForegroundService.TypeConnectedDevice)]
    public class AndroidForegroundService : Service
    {
        private bool _isStarted;
        private Task _startingTask = null;
        private Task _stoppingTask = null;
        private void Start()
        {
            lock (this)
            {
                if ((_startingTask == null || !_startingTask.IsCompletedSuccessfully) &&
                    (_stoppingTask == null || _stoppingTask.IsCompletedSuccessfully))
                {
                    var coreService = MauiApplication.Current.Services.GetService<ICoreService>();
                    _stoppingTask = null;
                    _startingTask = coreService.Start();
                }
            }
        }

        private void Stop()
        {
            lock (this)
            {
                if (_stoppingTask == null && _startingTask != null && _startingTask.IsCompletedSuccessfully)
                {
                    var coreService = MauiApplication.Current.Services.GetService<ICoreService>();
                    _startingTask = null;
                    _stoppingTask = coreService.Stop();
                }
            }
        }

        public override IBinder OnBind(Intent intent)
        {
            return new Binder();
        }

        private void RegisterForegroundService()
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

            var notification = new Builder(this, "background")
                .SetContentTitle("OmniCore")
                .SetContentText("OmniCore is running in the background.")
                // .SetContentIntent(showAppPendingIntent)
                .SetOngoing(true)
                .Build();

            // Enlist this instance of the service as a foreground service
            StartForeground(100, notification);
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            switch (intent.Action)
            {
                case "start":
                    if (!_isStarted)
                    {
                        RegisterForegroundService();
                        Start();
                        _isStarted = true;
                    }
                    return StartCommandResult.Sticky;
                case "stop":
                    if (_isStarted)
                    {
                        Stop();
                        _isStarted = false;
                        StopForeground(true);
                        StopSelf();
                    }
                    return StartCommandResult.NotSticky;
            }

            return StartCommandResult.ContinuationMask;
        }

        public override void OnDestroy()
        {
            if (_isStarted)
            {
                Stop();
                _isStarted = false;
                StopForeground(true);
                StopSelf();
            }
            base.OnDestroy();
        }
    }
}