﻿using System;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface INotifyStatus : IServiceInstance
    {
        NotifyStatusFlag StatusFlag { get; }
        string StatusMessage { get; }
        IObservable<INotifyStatus> WhenStatusUpdated();
    }
}