using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using OmniCore.Model.Extensions;
using OmniCore.Model.Interfaces.Common;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Base
{
    public abstract class BaseViewModel : IViewModel
    {
#pragma warning disable CS0067 // The event 'BaseViewModel.PropertyChanged' is never used
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067 // The event 'BaseViewModel.PropertyChanged' is never used

        protected ICoreServiceApi ServiceApi { get; set; }
        protected ICoreClient Client { get; set; }
        protected IView View { get; set; }

        protected IDisposable Subscription;
        protected List<IDisposable> DisposeList = new List<IDisposable>();

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
            DisposeDisposables();
        }

        public void InitializeModel(IView view)
        {
            View = view;
            Client.ClientConnection.WhenConnectionChanged().Subscribe(async (api) =>
            {
                ServiceApi = api;
                if (api != null)
                {
                    await OnInitialize();
                }
            }).AutoDispose(this);
        }

        public IList<IDisposable> Disposables { get; } = new List<IDisposable>(); 
        public void DisposeDisposables()
        {
            foreach(var disposable in Disposables)
                disposable.Dispose();

            Disposables.Clear();
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
            Client.ClientConnection.WhenConnectionChanged().Subscribe(async (api) =>
            {
                ServiceApi = api;
                if (api != null)
                {
                    await OnInitialize(parameter);
                }
            }).AutoDispose(this);
        }
    }
}
