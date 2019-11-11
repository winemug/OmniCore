using OmniCore.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RadioTestPage : ContentPage
    {
        private Radio RadioEntity => this.BindingContext as Radio;
        public RadioTestPage()
        {
            InitializeComponent();
        }

        private async void Connect_Clicked(object sender, EventArgs e)
        {
            var cts = new CancellationTokenSource();
            using (var connection = await App.Instance.RileyLinkProvider.GetConnection(RadioEntity, null, cts.Token))
            {
                var p = connection.PeripheralLease.Peripheral;
                await p.Connect(cts.Token);
            }
        }

        private async void Disconnect_Clicked(object sender, EventArgs e)
        {
            var cts = new CancellationTokenSource();
            using (var connection = await App.Instance.RileyLinkProvider.GetConnection(RadioEntity, null, cts.Token))
            {
                var p = connection.PeripheralLease.Peripheral;
                await p.Disconnect(TimeSpan.FromSeconds(3));
            }
        }

    }
}