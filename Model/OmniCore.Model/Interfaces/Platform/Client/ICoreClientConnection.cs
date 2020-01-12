using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface ICoreClientConnection : IClientResolvable
    {
        IObservable<ICoreServiceApi> WhenConnectionChanged();
    }
}