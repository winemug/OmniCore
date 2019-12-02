using OmniCore.Client.ViewModels.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.RadioTesting
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RadioTestPage : ContentPage
    {
        private RadioTestingViewModel ViewModel;
        public RadioTestPage()
        {
            InitializeComponent();
        }

        public RadioTestPage WithViewModel(RadioTestingViewModel viewModel)
        {
            ViewModel = viewModel;
            BindingContext = ViewModel;
            return this;
        }
        private async void Connect_Clicked(object sender, EventArgs e)
        {
            var cts = new CancellationTokenSource();
//            using (var connection = await XamarinApp.Instance.RileyLinkProvider.GetConnection(ViewModel.Radio, null, cts.Token))
//            {
//                var p = connection.PeripheralLease.Peripheral;
//                await p.Connect(true, cts.Token);
//            }
        }

        private async void Disconnect_Clicked(object sender, EventArgs e)
        {
            var cts = new CancellationTokenSource();
//            using (var connection = await XamarinApp.Instance.RileyLinkProvider.GetConnection(ViewModel.Radio, null, cts.Token))
//            {
//                var p = connection.PeripheralLease.Peripheral;
//                await p.Disconnect(TimeSpan.FromSeconds(3));
//            }
        }

    }
}