using System;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface ICoreNotification : IServerResolvable, IDisposable
    {
        int Id { get; }
        NotificationCategory Category { get; }
        string Title { get; }
        string Message { get; }
        TimeSpan? Timeout { get; }
        bool AutoDismiss { get; }
        void Update(string title, string message, TimeSpan? timeout = null);
        void Dismiss();
        IObservable<ICoreNotification> WhenDismissed();
         bool IsDismissed { get; }
         bool IsAutomaticallyDismissed  { get; }
        bool IsManuallyDismissed { get; }
    }
}
