using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OmniCore.Client.Annotations;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Client;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Home
{
    public class ServicePopupViewModel : IViewModel
    {
        private readonly ICoreClient Client;
        private IDisposable ApiStateSubscription;
        private IDisposable ConnectionSubscription;

        public ServicePopupViewModel(ICoreClient client)
        {
            Client = client;
            Disposables = new List<IDisposable>();
        }
        public IList<IDisposable> Disposables { get; }

        public void DisposeDisposables()
        {
        }

        public object Parameter { get; private set; }

        public void SetParameters(IView view, bool viaShell, object parameter)
        {
            Parameter = parameter;
            var page = (Page) view;
            page.BindingContext = this;
            ConnectionSubscription?.Dispose();
            ConnectionSubscription = Client.ClientConnection.WhenConnectionChanged()
                .Subscribe(api =>
                {
                    if (api == null)
                    {
                        ApiStateSubscription?.Dispose();
                        ApiStateSubscription = null;
                        // 
                    }
                    else
                    {
                        if (ApiStateSubscription == null)
                            ApiStateSubscription = api.ApiStatus.Subscribe(status =>
                            {
                                if (status == CoreApiStatus.Started)
                                {
                                    //
                                }
                            });
                    }
                });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}