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
using OmniCore.Mobile.Interfaces;
using Xamarin.Forms;
using OmniCore.Model.Utilities;

namespace OmniCore.Mobile.Android
{
    [Service(Exported = true, Enabled = true, Name ="somethingsomething.OmniCoreIntentService")]
    public class OmniCoreIntentService : IntentService
    {
        public const string ACTION_START_SERVICE = "OmniCoreIntentService.START_SERVICE";
        public const string ACTION_STOP_SERVICE = "OmniCoreIntentService.STOP_SERVICE";
        public const string ACTION_REQUEST_COMMAND = "OmniCoreIntentService.REQUEST_COMMAND";

        public const string NOTIFICATION_CHANNEL = "OmniCore";
        public const string NOTIFICATION_CHANNEL_NAME = "OmniCore";
        public const string NOTIFICATION_CHANNEL_DESCRIPTION = "OmniCore";

        private bool isStarted;

        private IOmniCoreLogger Logger;
        public OmniCoreIntentService()
        {
            Logger = DependencyService.Get<IOmniCoreLogger>();
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            Logger.Debug($"Service command received: {intent.Action}");
            if (intent.Action == ACTION_START_SERVICE && !isStarted)
            {
                Logger.Debug($"Starting foreground service");
                RegisterForegroundService();
                isStarted = true;
            }
            else if (intent.Action == ACTION_STOP_SERVICE && isStarted)
            {
                Logger.Debug($"Stopping foreground service");
                StopForeground(true);
                StopSelf();
                isStarted = false;
            }
            else if (intent.Action == ACTION_REQUEST_COMMAND)
            {
                Logger.Debug($"handling execute request");
                HandleRequest(intent);
            }

            return StartCommandResult.Sticky;
        }

        private void HandleRequest(Intent intent)
        {
                var request = intent.GetStringExtra("request");
                var messenger = intent.GetParcelableExtra("messenger") as Messenger;
                var publisher = DependencyService.Get<IRemoteRequestPublisher>(DependencyFetchTarget.GlobalInstance);
                try
                {
                    var t = Task.Run(async () =>
                    {
                        try
                        {
                            var resultTask = publisher.GetResult(request);
                            while (true)
                            {
                                var tr = await Task.WhenAny(resultTask, Task.Delay(5000)).NoSync();
                                if (tr == resultTask)
                                    break;
                                var bb = new Bundle();
                                Logger.Verbose("Sending busy / keep-alive");
                                bb.PutBoolean("busy", true);
                                messenger.Send(new Message { Data = bb });
                            }
                            var result = await resultTask.NoSync();
                            var b = new Bundle();
                            b.PutBoolean("finished", true);
                            b.PutString("response", result);
                            Logger.Verbose("Responding to request via message object");
                            messenger.Send(new Message { Data = b });
                            Logger.Verbose("Message send complete");
                        }
                        catch (Exception e)
                        {
                            Logger.Error("Error handling remote request", e);
                        }
                    });
                }
                catch (AggregateException ae)
                {
                    Logger.Error("Error handling remote request", ae.Flatten());
                }
                catch (Exception e)
                {
                    Logger.Error("Error handling remote request", e);
                }
        }

        private void RegisterForegroundService()
        {
            try
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
            catch(Exception e)
            {
                Logger.Error("Error registering foreground service", e);
            }
        }

        protected override void OnHandleIntent(Intent intent)
        {
        }
    }
}