using System;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Model.Interfaces.Client
{
    public interface IClientConnection : IClientInstance
    {
        IObservable<IClientConnection> WhenDisconnected();
        IObservable<IApi> WhenConnected();
        Task Connect();
        Task Disconnect();
    }
}