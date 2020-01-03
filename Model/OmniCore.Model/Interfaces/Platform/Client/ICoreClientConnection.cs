using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces;

namespace OmniCore.Model.Interfaces
{
    public interface ICoreClientConnection : IClientResolvable
    {
        IObservable<ICoreServiceApi> WhenConnectionChanged();
    }
}