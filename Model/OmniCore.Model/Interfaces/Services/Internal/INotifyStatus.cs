using System;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface INotifyStatus 
    {
        NotifyStatusFlag StatusFlag { get; }
        string StatusMessage { get; }
        IObservable<INotifyStatus> WhenStatusUpdated();
    }
}