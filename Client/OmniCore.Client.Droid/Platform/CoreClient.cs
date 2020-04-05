using System;
using System.Threading;
using Android.App;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Client.Droid
{
    public class CoreClient : ICoreClient
    {
        public CoreClient(ICoreContainer<IClientResolvable> clientContainer,
            ICoreClientConnection connection,
            IViewPresenter viewPresenter)
        {
            ClientContainer = clientContainer;
            ClientConnection = connection;
            ViewPresenter = viewPresenter;
        }

        public ICoreContainer<IClientResolvable> ClientContainer { get; }

        public IViewPresenter ViewPresenter { get; }

        public SynchronizationContext SynchronizationContext => Application.SynchronizationContext;

        public ICoreClientConnection ClientConnection { get; }

        public IDisposable DisplayKeepAwake()
        {
            return new KeepAwakeLock();
        }
    }
}