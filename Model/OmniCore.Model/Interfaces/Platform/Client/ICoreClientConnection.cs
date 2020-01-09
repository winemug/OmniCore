using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Interfaces.Common
{
    public interface ICoreClientConnection : IClientResolvable
    {
        IObservable<ICoreServiceApi> WhenConnectionChanged();
    }
}