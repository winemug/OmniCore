using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface ICoreClientConnection : IClientResolvable
    {
        IObservable<ICoreServiceApi> WhenConnectionChanged();
    }
}