using System;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ICoreNotificationFunctions : ICoreServerFunctions
    {
        ICoreNotification CreateNotification(NotificationCategory category, string title, string message,
            TimeSpan? timeout = null, bool autoDismiss = true);

        void ClearNotifications();
        IObservable<ICoreNotification> WhenNotificationAdded();
        IObservable<ICoreNotification> WhenNotificationDismissed();
    }
}