using System;
using OmniCore.Model.Interfaces.Base;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Model.Interfaces.Client
{
    public interface ICoreClientConnection : IClientResolvable
    {
        IObservable<ICoreApi> WhenConnectionChanged();
    }
}