using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using OmniCore.Model.Interfaces.Platform;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Base
{
    public abstract class BaseViewModel : IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected ICoreServices Services { get; set; }
        protected ICoreClient Client { get; set; }
        protected IView View { get; set; }

        protected IDisposable Subscription;

        protected virtual Task OnInitialize()
        {
            return Task.CompletedTask;
        }

        protected virtual void OnDispose()
        {
        }

        public BaseViewModel(ICoreClient client)
        {
            Client = client;
        }

        public void Dispose()
        {
            OnDispose();
        }

        public void InitializeModel(IView view)
        {
            View = view;
            Subscription?.Dispose();
            Subscription = Client.ServicesConnection.WhenConnectionChanged().Subscribe(async (services) =>
            {
                Services = services;
                if (services != null)
                {
                    await OnInitialize();
                }
            });
        }
    }

    public abstract class BaseViewModel<TParameter> : BaseViewModel, IViewModel<TParameter>
    {
        public abstract Task OnInitialize(TParameter parameter);
        public BaseViewModel(ICoreClient client) : base(client)
        {
        }
        protected override Task OnInitialize()
        {
            throw new InvalidOperationException();
        }

        public void InitializeModel(IView view, TParameter parameter)
        {
            View = view;
            Subscription?.Dispose();
            Subscription = Client.ServicesConnection.WhenConnectionChanged().Subscribe(async (services) =>
            {
                Services = services;
                if (services != null)
                {
                    await OnInitialize();
                }
            });
        }
    }
}
