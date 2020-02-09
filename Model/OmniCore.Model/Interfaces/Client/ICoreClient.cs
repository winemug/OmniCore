using System;
using System.Threading;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Interfaces.Client
{
    public interface ICoreClient : ICoreClientFunctions
    {
        ICoreContainer<IClientResolvable> ClientContainer { get; }
        IViewPresenter ViewPresenter { get; }
        ICoreClientConnection ClientConnection { get; }
        SynchronizationContext SynchronizationContext { get; }
        IDisposable DisplayKeepAwake();
    }
}