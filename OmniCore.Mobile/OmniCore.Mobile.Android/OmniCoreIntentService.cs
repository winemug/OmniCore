using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using OmniCore.Mobile.Interfaces;
using Xamarin.Forms;

namespace OmniCore.Mobile.Droid
{
    [Service(Exported = true, Name = "OmniCore.Mobile.Droid.OmniCoreIntentService", Enabled = true)]
    public class OmniCoreIntentService : IntentService, ILocalRequestPublisher
    {
        public const string ACTION_START_SERVICE = "OmniCoreIntentService.START_SERVICE";
        public const string ACTION_STOP_SERVICE = "OmniCoreIntentService.STOP_SERVICE";
        public const string NOTIFICATION_CHANNEL = "OmniCore";
        public const string NOTIFICATION_CHANNEL_NAME = "OmniCore";
        public const string NOTIFICATION_CHANNEL_DESCRIPTION = "OmniCore";

        private List<ILocalRequestSubscriber> Subscribers;
        private bool isStarted;

        public OmniCoreIntentService() : base("OmniCoreIntentService")
        {
            Subscribers = new List<ILocalRequestSubscriber>();
        }
        
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (intent.Action.Equals(ACTION_START_SERVICE) && !isStarted)
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

            return StartCommandResult.Sticky;
        }

        protected async override void OnHandleIntent(Intent intent)
        {
            var intentText = intent.GetStringExtra("request");
            foreach (var subscriber in Subscribers)
                await subscriber.OnRequestReceived(intentText);
        }

        public void Subscribe(ILocalRequestSubscriber subscriber)
        {
            Subscribers.Add(subscriber);
        }

        public void Unsubscribe(ILocalRequestSubscriber subscriber)
        {
            Subscribers.Remove(subscriber);
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

            NotificationCompat.Builder builder = new NotificationCompat.Builder(this, NOTIFICATION_CHANNEL)
                .SetContentTitle("OmniCore")
                .SetContentText("OmniCore is running")
                .SetSmallIcon(Resource.Drawable.ic_pod);

            var notification = builder.Build();

            StartForeground(10001, notification);
        }
    }
}