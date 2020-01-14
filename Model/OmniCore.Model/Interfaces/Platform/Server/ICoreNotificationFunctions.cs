using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Server;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ICoreNotificationFunctions : ICoreServerFunctions
    {
        ICoreNotification CreateNotification(NotificationCategory category, string title, string message);
        void ClearNotifications();
        IObservable<ICoreNotification> WhenNotificationAdded();
        IObservable<ICoreNotification> WhenNotificationDismissed();
    }
}
