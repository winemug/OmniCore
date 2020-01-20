using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Android.App;
using Android.Content;
using Android.Support.V4.App;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Platform.Server;

namespace OmniCore.Client.Droid.Platform
{
    public class CoreNotification : ICoreNotification
    {
        public int Id { get; private set; }
        public NotificationCategory Category { get; private set; }
        public string Title { get; private set; }
        public string Message { get; private set; }
        public TimeSpan? Timeout { get; private set; }
        public bool AutoDismiss { get; private set; }
        public Notification NativeNotification { get; private set; }

        public bool IsDismissed => IsAutomaticallyDismissed || IsManuallyDismissed;
        public bool IsAutomaticallyDismissed  { get; private set; }
        public bool IsManuallyDismissed { get; private set; }
        
        private Context Context;
        private ISubject<ICoreNotification> DismissSubject;

        public void CreateInternal(
            Context context,
            int id,
            NotificationCategory category,
            string title,
            string message,
            TimeSpan? timeout = null,
            bool autoDismiss = true)
        {
            Id = id;
            Category = category;
            Title = title;
            Message = message;
            Timeout = timeout;
            AutoDismiss = autoDismiss;
            Context = context;

            DismissSubject = new AsyncSubject<ICoreNotification>();
            
            SetNotification();
        }
        public void Update(string title, string message, TimeSpan? timeout = null)
        {
            lock (this)
            {
                Title = title;
                Message = message;
                Timeout = timeout;

                SetNotification();
                var notificationManager = (NotificationManager)
                    Context.GetSystemService(Context.NotificationService);

                notificationManager.Notify(Id, NativeNotification);
            }
        }

        public void Dismiss()
        {
            lock (this)
            {
                if (NativeNotification != null)
                {
                    var notificationManager = (NotificationManager)
                        Context.GetSystemService(Context.NotificationService);

                    notificationManager.Cancel(Id);
                    IsManuallyDismissed = true;
                    NativeNotification.Dispose();
                    NativeNotification = null;
                    DismissSubject.OnNext(this);
                }
            }
        }

        public IObservable<ICoreNotification> WhenDismissed()
        {
            return DismissSubject.AsObservable();
        }
        
        private void SetNotification()
        {
            var notificationManager = (NotificationManager)
                Context.GetSystemService(Context.NotificationService);
#pragma warning disable CS0618 // 'Notification.Builder.Builder(Context)' is obsolete: 'deprecated'

            var notificationBuilder = new Notification.Builder(Context)
#pragma warning restore CS0618 // 'Notification.Builder.Builder(Context)' is obsolete: 'deprecated'
                .SetSmallIcon(Resource.Drawable.ic_stat_pod);
            if (!string.IsNullOrEmpty(Title))
                notificationBuilder.SetContentTitle(Title);
            if (!string.IsNullOrEmpty(Message))
            {
                notificationBuilder.SetStyle(new Notification.BigTextStyle());
                notificationBuilder.SetContentText(Message);
            }

            notificationBuilder.SetChannelId(Category.ToString("G"));
            notificationBuilder.SetOnlyAlertOnce(true);
            notificationBuilder.SetAutoCancel(AutoDismiss);
            if (Timeout.HasValue)
                notificationBuilder.SetTimeoutAfter((long)Timeout.Value.TotalMilliseconds);
           
            NativeNotification?.Dispose();
            NativeNotification = notificationBuilder.Build();
            notificationManager.Notify(Id, NativeNotification);
        }

        public void Dispose()
        {
            Dismiss();
        }
    }
}