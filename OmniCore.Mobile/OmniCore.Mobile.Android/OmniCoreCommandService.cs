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
using Android.Views;
using Android.Widget;
using Microsoft.AppCenter.Crashes;
using OmniCore.Mobile.Base;
using OmniCore.Model.Eros;

namespace OmniCore.Mobile.Android
{
    [Service(Name = "OmniCore.CommandService", Enabled = true, Exported = true, Process="net.balya.OmniCore.Mobile.Android")]
    public class OmniCoreCommandService : Service
    {
        public const string NOTIFICATION_CHANNEL = "OmniCoreCommandService";
        public const string NOTIFICATION_CHANNEL_NAME = "OmniCoreCommandService";
        public const string NOTIFICATION_CHANNEL_DESCRIPTION = "OmniCoreCommandService";
        public const int NOTIFICATION_ID = 10001;

        private Messenger ServiceMessenger;

        private OmniCoreLogger Logger = new OmniCoreLogger();

        public override IBinder OnBind(Intent intent)
        {
            ServiceMessenger = new Messenger(new OmniCoreCommandHandler());
            return ServiceMessenger.Binder;
        }

        public override void OnCreate()
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
                    .SetSmallIcon(Resource.Mipmap.ic_omnicore);

                var notification = builder.Build();

                StartForeground(NOTIFICATION_ID, notification);
            }
            catch (Exception e)
            {
                OmniCoreServices.Logger.Error("Error registering foreground service", e);
                Crashes.TrackError(e);
            }
            base.OnCreate();
        }

        public override void OnDestroy()
        {
            StopForeground(true);
            StopSelf();
            base.OnDestroy();
        }
    }

    public class OmniCoreCommandHandler : Handler
    {
        public override void HandleMessage(Message msg)
        {
            lock (this)
            {
                var messenger = msg.Data.GetParcelable("responseMessenger") as Messenger;
                var request = msg.Data.GetString("request", null);
                if (messenger == null || request == null)
                    return;
                if (!MainActivity.IsCreated)
                {
                    try
                    {
                        var b = new Bundle();
                        b.PutBoolean("initialized", false);
                        b.PutBoolean("finished", false);
                        b.PutString("response", null);
                        messenger.Send(new Message { Data = b });
                    }
                    catch (Exception e)
                    {
                        Crashes.TrackError(e);
                    }
                }
                else
                {
                    HandleRequest(request, messenger);
                }
            }
        }

        private void HandleRequest(string request, Messenger messenger)
        {
            try
            {
                var t = Task.Run(async () => { await HandleRequestAsync(request, messenger); });
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

        private async Task HandleRequestAsync(string request, Messenger messenger)
        {
            try
            {
                using (var wakeLock = OmniCoreServices.Application.NewBluetoothWakeLock("OmniCoreServiceHandlerLock"))
                {
                    if (!await wakeLock.Acquire(5000))
                    {
                        OmniCoreServices.Logger.Verbose("Wakelock acquisition failed, sending null response");
                        var b = new Bundle();
                        b.PutBoolean("initialized", true);
                        b.PutBoolean("finished", false);
                        b.PutString("response", null);
                        messenger.Send(new Message { Data = b });
                        OmniCoreServices.Logger.Verbose("Message send complete");
                        return;
                    }

                    var repo = await ErosRepository.GetInstance();
                    var ocs = await repo.GetOmniCoreSettings();
                    if (ocs.AcceptCommandsFromAAPS)
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
            }
            catch (Exception e)
            {
                OmniCoreServices.Logger.Error("Error handling remote request", e);
                Crashes.TrackError(e);
            }
        }
    }
}