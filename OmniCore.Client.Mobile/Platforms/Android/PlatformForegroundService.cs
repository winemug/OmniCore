using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using OmniCore.Client.Interfaces.Services;

namespace OmniCore.Client.Mobile.Platforms.Android;
public class PlatformForegroundService : IPlatformForegroundService
{
    public Task<IDisposable> RunInForegroundAsync()
    {
        throw new NotImplementedException();
    }
}

// Android 34
// android.permission.FOREGROUND_SERVICE_SYSTEM_EXEMPTED
// FOREGROUND_SERVICE_TYPE_SYSTEM_EXEMPTED = 1024
[Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeDataSync |
                                 global::Android.Content.PM.ForegroundService.TypeConnectedDevice)]
public class AndroidForegroundService : Service
{
    private bool _isStarted;

    private void Start()
    {

    }

    private void Stop()
    {

    }
    private void RegisterForegroundService()
    {
        //if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        //{
        //    var notificationManager = GetSystemService(NotificationService) as NotificationManager;
        //    var channel = new NotificationChannel("background", "Background notification",
        //        NotificationImportance.Low);
        //    notificationManager.CreateNotificationChannel(channel);
        //}


        // this.PackageManager.GetLaunchIntentForPackage()
        // var launchIntent = new Intent(this, typeof(MainActivity));
        // showAppIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTask);
        // var showAppPendingIntent = PendingIntent.GetActivity(this, 0, showAppIntent, PendingIntentFlags.UpdateCurrent);

        //var notification = new Builder(this, "background")
        //    .SetContentTitle("OmniCore")
        //    .SetContentText("OmniCore is running in the background.")
        //    // .SetContentIntent(showAppPendingIntent)
        //    .SetOngoing(true)
        //    .Build();

        // Enlist this instance of the service as a foreground service
        //StartForeground(100, notification);
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
    public override IBinder OnBind(Intent intent)
    {
        return new Binder();
    }
}