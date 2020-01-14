using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Model.Interfaces.Platform.Server
{
    public interface ICoreNotification : IServerResolvable, IDisposable
    {
        int Id { get; }
        NotificationCategory Category { get; }
        string Title { get; }
        string Message { get; }
        TimeSpan? Timeout { get; }
        bool AutoDismiss { get; }
        void Update(string title, string message, TimeSpan? timeout);
        void Dismiss();
        IObservable<ICoreNotification> WhenDismissed();
         bool IsDismissed { get; }
         bool IsAutomaticallyDismissed  { get; }
        bool IsManuallyDismissed { get; }
    }
}
