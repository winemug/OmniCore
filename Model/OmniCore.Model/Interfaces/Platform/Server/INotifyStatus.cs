using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Model.Interfaces.Platform.Server
{
    public interface INotifyStatus : IServerResolvable
    {
        NotifyStatusFlag StatusFlag { get; }
        string StatusMessage { get; }
        IObservable<INotifyStatus> WhenStatusUpdated();
    }
}
