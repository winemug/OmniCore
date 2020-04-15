using System;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Model.Interfaces.Client
{
    public interface ICoreClientConnection : IClientResolvable
    {
        IObservable<ICoreClientConnection> WhenDisconnected();
        IObservable<ICoreApi> WhenConnected();
        Task Connect();
        Task Disconnect();
    }
}