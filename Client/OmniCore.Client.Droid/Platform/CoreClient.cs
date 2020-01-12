using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Platform.Client;

namespace OmniCore.Client.Droid
{
    public class CoreClient : ICoreClient
    {
        public ICoreContainer<IClientResolvable> ClientContainer { get; }

        public IViewPresenter ViewPresenter { get; }

        public SynchronizationContext SynchronizationContext => Application.SynchronizationContext;

        public ICoreClientConnection ClientConnection { get; }

        public CoreClient(ICoreContainer<IClientResolvable> clientContainer,
            ICoreClientConnection connection,
            IViewPresenter viewPresenter)
        {
            ClientContainer = clientContainer;
            ClientConnection = connection;
            ViewPresenter = viewPresenter;
        }

        public IDisposable DisplayKeepAwake()
        {
            return new KeepAwakeLock();
        }

    }
}