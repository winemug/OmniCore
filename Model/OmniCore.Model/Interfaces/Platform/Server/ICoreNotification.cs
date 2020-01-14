using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Model.Interfaces.Platform.Server
{
    public interface ICoreNotification : IServerResolvable
    {
        int Id { get; }
        NotificationCategory Category { get; }
        string Title { get; }
        string Message { get; }

        void Update(string title, string message);
        void Dismiss();
        IObservable<ICoreNotification> WhenDismissed();
    }
}
