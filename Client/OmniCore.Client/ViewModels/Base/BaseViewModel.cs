using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Services;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Base
{
    public abstract class BaseViewModel : IViewModel
    {
#pragma warning disable CS0067 // The event 'BaseViewModel.PropertyChanged' is never used
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067 // The event 'BaseViewModel.PropertyChanged' is never used

        protected ICoreApi Api { get; set; }
        protected ICoreClient Client { get; }

        public IView View { get; protected set; }
        public object Parameter { get; protected set; }

        private bool ViaShell = false;

        public BaseViewModel(ICoreClient client)
        {
            Client = client;
        }

        protected virtual Task OnPageAppearing()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnPageDisappearing()
        {
            return Task.CompletedTask;
        }

        public IList<IDisposable> Disposables { get; } = new List<IDisposable>(); 

        public void DisposeDisposables()
        {
            foreach(var disposable in Disposables)
                disposable.Dispose();

            Disposables.Clear();
        }

        public void SetParameters(IView view, bool viaShell, object parameter)
        {
            ViaShell = viaShell;
            Parameter = parameter;
            View = view;
            Parameter = parameter;
            var page = (Page) view;
            page.Appearing += PageAppearing;
            page.Disappearing += PageDisappearing;
            page.BindingContext = this;
        }

        private async void PageAppearing(object sender, EventArgs args)
        {
            Api = await Client.ClientConnection.WhenConnectionChanged().FirstAsync(c => c != null);
            await OnPageAppearing();
        }

        private async void PageDisappearing(object sender, EventArgs args)
        {
            await OnPageDisappearing();
            DisposeDisposables();
            if (!ViaShell)
            {
                var page = (Page) View;
                page.Appearing -= PageAppearing;
                page.Disappearing -= PageDisappearing;
            }
        }
    }
}
