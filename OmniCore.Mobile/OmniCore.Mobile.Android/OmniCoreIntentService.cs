using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.IO;
using OmniCore.Mobile.Droid;
using OmniCore.Mobile.Interfaces;
using Xamarin.Forms;

namespace OmniCore.Mobile.Droid
{
    [Service(Exported = true, Enabled = true, Name = "net.balya.OmniCore.Mobile.Android.OmniCoreIntentService")]
    public class OmniCoreIntentService : IntentService
    {
        public const string ACTION_START_SERVICE = "OmniCoreIntentService.START_SERVICE";
        public const string ACTION_STOP_SERVICE = "OmniCoreIntentService.STOP_SERVICE";
        public const string ACTION_REQUEST_COMMAND = "OmniCoreIntentService.REQUEST_COMMAND";
        public const string NOTIFICATION_CHANNEL = "OmniCore";
        public const string NOTIFICATION_CHANNEL_NAME = "OmniCore";
        public const string NOTIFICATION_CHANNEL_DESCRIPTION = "OmniCore";

        private bool isStarted;
        
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (intent.Action == ACTION_START_SERVICE && !isStarted)
            {
                RegisterForegroundService();
                isStarted = true;
            }
            else if (intent.Action == ACTION_STOP_SERVICE && isStarted)
            {
                StopForeground(true);
                StopSelf();
                isStarted = false;
            }
            else if (intent.Action == ACTION_REQUEST_COMMAND)
            {
                Task.Run( async () => await HandleRequest(intent));
            }

            return StartCommandResult.Sticky;
        }

        private async Task HandleRequest(Intent intent)
        {
            var request = intent.GetStringExtra("request");
            var messenger = intent.GetParcelableExtra("messenger") as Messenger;
            var response = await DependencyService
                .Get<IRemoteRequestPublisher>(DependencyFetchTarget.GlobalInstance)
                .GetResult(request);
            var b = new Bundle();
            b.PutString("response", response);
            messenger.Send(new Message { Data = b });
        }

        private void RegisterForegroundService()
        {
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(NOTIFICATION_CHANNEL, NOTIFICATION_CHANNEL_NAME, NotificationImportance.Default)
                {
                    Description = NOTIFICATION_CHANNEL_DESCRIPTION
                };

                notificationManager.CreateNotificationChannel(channel);
            }

            var intent = new Intent(this, typeof(MainActivity));
            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.UpdateCurrent);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(this, NOTIFICATION_CHANNEL)
                .SetContentIntent(pendingIntent)
                .SetContentTitle("OmniCore")
                .SetContentText("OmniCore is running")
                .SetSmallIcon(Resource.Drawable.ic_pod);

            var notification = builder.Build();

            StartForeground(10001, notification);
        }

        protected override void OnHandleIntent(Intent intent)
        {
        }
    }
}