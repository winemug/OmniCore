using System;
using Android.App;
using Android.Content;
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
        public Notification NativeNotification { get; private set; }

        private Context Context;

        public void CreateInternal(
            Context context,
            int id,
            NotificationCategory category,
            string title,
            string message)
        {
            Id = id;
            Category = category;
            Title = title;
            Message = message;
            
            var notificationManager = (NotificationManager)
                context.GetSystemService(Context.NotificationService);
#pragma warning disable CS0618 // 'Notification.Builder.Builder(Context)' is obsolete: 'deprecated'
            var notificationBuilder = new Notification.Builder(context)
#pragma warning restore CS0618 // 'Notification.Builder.Builder(Context)' is obsolete: 'deprecated'
                .SetSmallIcon(Resource.Drawable.ic_stat_pod);
            if (!string.IsNullOrEmpty(title))
                notificationBuilder.SetContentTitle(title);
            if (!string.IsNullOrEmpty(message))
                notificationBuilder.SetContentText(message);

            notificationBuilder.SetCategory(category.ToString());
            
            var notification = notificationBuilder.Build();
            NativeNotification = notification;
            notificationManager.Notify(id, notification);
        }

        public void Update(string title, string message)
        {
            Title = title;
            Message = message;
            
            var notificationManager = (NotificationManager)
                Context.GetSystemService(Context.NotificationService);
            notificationManager.Notify(Id, NativeNotification);
        }

        public void Dismiss()
        {
            var notificationManager = (NotificationManager)
                Context.GetSystemService(Context.NotificationService);
            
            throw new NotImplementedException();
        }

        public IObservable<ICoreNotification> WhenDismissed()
        {
            throw new NotImplementedException();
        }
    }
}