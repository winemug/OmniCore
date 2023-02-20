using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using OmniCore.Services.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Unity;
using Xamarin.Forms;
using Debug = System.Diagnostics.Debug;

namespace OmniCore.Mobile.Droid
{
    [Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeDataSync | Android.Content.PM.ForegroundService.TypeConnectedDevice)]
    public class ForegroundService : Service
    {
        private bool IsStarted = false;
        // private Task ServiceTask = null;
        // private CancellationTokenSource ServiceCts = null;

        private IConnection Connection;

        public ForegroundService()
        {
            Debug.WriteLine("Service Constructor");
        }

        private void Start()
        {
            var fsh = DependencyService.Resolve<IForegroundServiceHelper>();
            fsh.Service?.Start();
        }

        private void Stop()
        {
            var fsh = DependencyService.Resolve<IForegroundServiceHelper>();
            fsh.Service?.Stop();
        }

        public override void OnCreate()
        {
            base.OnCreate();
            Debug.WriteLine("Service On Create");
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
       
        void RegisterForegroundService()
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

            var notification = new Notification.Builder(this, "background")
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
            Debug.WriteLine($"Service On Start Command action: {intent.Action}");

            switch (intent.Action)
            {
                case "start":
                    if (!IsStarted)
                    {
                        RegisterForegroundService();
                        Start();
                        IsStarted = true;
                    }
                    break;
                case "stop":
                    if (IsStarted)
                    {
                        Stop();
                        IsStarted = false;
                        StopForeground(true);
                        StopSelf();
                    }
                    break;
            }


            // App.Container.Resolve<IForegroundDataService>().StartRequested();

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            Debug.WriteLine("Service On Destroy");
            if (IsStarted)
            {
                Stop();
                IsStarted = false;
            }
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Cancel(100);
            // App.Container.Resolve<IForegroundDataService>().StopRequested();
            base.OnDestroy();
        }
    }
}