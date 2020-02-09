using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Client;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Home
{
    public class ServicePopupViewModel : IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public IList<IDisposable> Disposables { get; }
        private readonly ICoreClient Client;
        private IDisposable ConnectionSubscription;
        private IDisposable ApiStateSubscription;

        public ServicePopupViewModel(ICoreClient client)
        {
            Client = client;
        }

        public void DisposeDisposables()
        {
        }

        public object Parameter { get; }
        public void SetParameters(IView view, bool viaShell, object parameter)
        {
            var page = (Page) view;
            page.BindingContext = this;
            ConnectionSubscription?.Dispose();
            ConnectionSubscription = Client.ClientConnection.WhenConnectionChanged()
                .Subscribe(async api =>
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
                        {
                            ApiStateSubscription = api.ApiStatus.Subscribe(async status =>
                            {
                                if (status == CoreApiStatus.Started)
                                {
                                    //
                                }
                                else
                                {
                                    //
                                }
                            });
                        }
                    }
                });
        }
    }
}
