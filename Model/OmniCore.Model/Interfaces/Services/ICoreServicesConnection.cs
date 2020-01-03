using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface ICoreServicesConnection : IClientResolvable
    {
        IObservable<ICoreServices> WhenConnectionChanged();
    }
}