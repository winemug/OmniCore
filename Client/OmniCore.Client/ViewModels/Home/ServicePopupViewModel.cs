using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OmniCore.Client.Annotations;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Home
{
    public class ServicePopupViewModel : BaseViewModel
    {

        public ServicePopupViewModel(IClient client) : base(client)
        {
        }
        public void Initialize(IView view, bool viaShell, object parameter)
        {
            // Parameter = parameter;
            // var page = (Page) view;
            // page.BindingContext = this;
            // ConnectionSubscription?.Dispose();
            // ConnectionSubscription = Client.ClientConnection.WhenConnectionChanged()
            //     .Subscribe(api =>
            //     {
            //         if (api == null)
            //         {
            //             ApiStateSubscription?.Dispose();
            //             ApiStateSubscription = null;
            //             // 
            //         }
            //         else
            //         {
            //             if (ApiStateSubscription == null)
            //                 ApiStateSubscription = api.ApiStatus.Subscribe(status =>
            //                 {
            //                     if (status == CoreApiStatus.Started)
            //                     {
            //                         //
            //                     }
            //                 });
            //         }
            //     });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}