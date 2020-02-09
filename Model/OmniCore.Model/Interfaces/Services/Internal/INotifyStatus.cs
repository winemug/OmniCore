using System;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Base;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface INotifyStatus : IServerResolvable
    {
        NotifyStatusFlag StatusFlag { get; }
        string StatusMessage { get; }
        IObservable<INotifyStatus> WhenStatusUpdated();
    }
}
