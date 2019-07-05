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
using OmniCore.Mobile.Base.Interfaces;
using Xamarin.Forms;
using OmniCore.Model.Utilities;
using OmniCore.Mobile.Base;
using OmniCore.Model.Eros;
using Microsoft.AppCenter.Crashes;

namespace OmniCore.Mobile.Android
{
    [Service(Exported = true, Enabled = true, Name ="OmniCore.IntentService")]
    public class OmniCoreIntentService : IntentService
    {
        public const string ACTION_START_SERVICE = "OmniCoreIntentService.START_SERVICE";
        public const string ACTION_STOP_SERVICE = "OmniCoreIntentService.STOP_SERVICE";
        public const string ACTION_REQUEST_COMMAND = "OmniCoreIntentService.REQUEST_COMMAND";

        public const string NOTIFICATION_CHANNEL = "OmniCore";
        public const string NOTIFICATION_CHANNEL_NAME = "OmniCore";
        public const string NOTIFICATION_CHANNEL_DESCRIPTION = "OmniCore";
        public const int NOTIFICATION_ID = 10001;

        private bool isStarted;

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (intent == null)
            {
                RegisterForegroundService();
                isStarted = true;
                return StartCommandResult.Sticky;
            }

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
                if (!MainActivity.IsCreated)
                {
                    try
                    {
                        var messenger = intent.GetParcelableExtra("messenger") as Messenger;
                        var b = new Bundle();
                        b.PutBoolean("initialized", false);
                        b.PutBoolean("finished", false);
                        b.PutString("response", null);
                        messenger.Send(new Message { Data = b });
                    }
                    catch(Exception e)
                    {
                        Crashes.TrackError(e);
                    }
                    return StartCommandResult.NotSticky;
                }
                else
                {
                    OmniCoreServices.Logger.Debug($"handling execute request");
                    HandleRequest(intent);
                }
            }

            return StartCommandResult.Sticky;
        }

        protected override void OnHandleIntent(Intent intent)
        {
        }

        private void HandleRequest(Intent intent)
        {
            lock (this)
            {
                var request = intent.GetStringExtra("request");
                var messenger = intent.GetParcelableExtra("messenger") as Messenger;
                try
                {
                    var t = Task.Run(async () =>
                    {
                        IWakeLock wakeLock = null;
                        try
                        {
                            wakeLock = OmniCoreServices.Application.NewBluetoothWakeLock("OmniCoreServiceHandler");
                            if (!await wakeLock.Acquire(5000))
                            {
                                OmniCoreServices.Logger.Verbose("Wakelock acquisition failed, sending null response");
                                var b = new Bundle();
                                b.PutBoolean("initialized", true);
                                b.PutBoolean("finished", false);
                                b.PutString("response", null);
                                messenger.Send(new Message { Data = b });
                                OmniCoreServices.Logger.Verbose("Message send complete");
                            }
                            if (ErosRepository.Instance.GetOmniCoreSettings().AcceptCommandsFromAAPS)
                            {
                                var resultTask = OmniCoreServices.Publisher.GetResult(request);
                                while (true)
                                {
                                    var tr = await Task.WhenAny(resultTask, Task.Delay(5000));
                                    if (tr == resultTask)
                                        break;
                                    var bb = new Bundle();
                                    OmniCoreServices.Logger.Verbose("Sending busy / keep-alive");
                                    bb.PutBoolean("busy", true);
                                    messenger.Send(new Message { Data = bb });
                                }
                                var result = await resultTask;
                                var b = new Bundle();
                                b.PutBoolean("initialized", true);
                                b.PutBoolean("finished", true);
                                b.PutString("response", result);
                                OmniCoreServices.Logger.Verbose("Responding to request via message object");
                                messenger.Send(new Message { Data = b });
                                OmniCoreServices.Logger.Verbose("Message send complete");
                            }
                            else
                            {
                                OmniCoreServices.Logger.Verbose("Ignoring AAPS command");
                                await Task.Delay(30000);
                                var b = new Bundle();
                                b.PutBoolean("finished", true);
                                b.PutString("response", null);
                                messenger.Send(new Message { Data = b });
                                OmniCoreServices.Logger.Verbose("Message send complete");
                            }
                        }
                        catch (Exception e)
                        {
                            OmniCoreServices.Logger.Error("Error handling remote request", e);
                            Crashes.TrackError(e);
                        }
                        finally
                        {
                            wakeLock?.Dispose();
                        }
                    });
                }
                catch (AggregateException ae)
                {
                    OmniCoreServices.Logger.Error("Error handling remote request", ae.Flatten());
                    Crashes.TrackError(ae);
                }
                catch (Exception e)
                {
                    OmniCoreServices.Logger.Error("Error handling remote request", e);
                    Crashes.TrackError(e);
                }
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
                var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.Immutable);

                NotificationCompat.Builder builder = new NotificationCompat.Builder(this, NOTIFICATION_CHANNEL)
                    .SetContentIntent(pendingIntent)
                    .SetContentTitle("OmniCore")
                    .SetContentText("OmniCore is running")
                    .SetSmallIcon(Resource.Drawable.ic_pod);

                var notification = builder.Build();

                StartForeground(NOTIFICATION_ID, notification);
            }
            catch(Exception e)
            {
                OmniCoreServices.Logger.Error("Error registering foreground service", e);
                Crashes.TrackError(e);
            }
        }
    }
}