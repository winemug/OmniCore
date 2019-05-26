using OmniCore.Data;
using OmniCore.Mobile.ViewModels;
using OmniCore.Model;
using OmniCore.Model.Eros;
using OmniCore.Radio.RileyLink;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Mobile.Views
{
    public partial class TestPage : ContentPage
    {
        TestViewModel viewModel;

        ErosPod Pod;

        public TestPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new TestViewModel();
            var exchangeProvider = new RileyLinkProvider();
            Pod = new ErosPod(exchangeProvider, DataStore.Instance);
        }

        private async Task<bool> CheckPermission(Permission p)
        {
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(p);
            if (status != PermissionStatus.Granted)
            {
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(p))
                {
                    await DisplayAlert("Needed", "Gonna need that permission son", "OK");
                }

                var results = await CrossPermissions.Current.RequestPermissionsAsync(p);
                //Best practice to always check that the key exists
                if (results.ContainsKey(p))
                    status = results[p];
            }

            if (status == PermissionStatus.Granted)
            {
                return true;
            }
            else if (status != PermissionStatus.Unknown)
            {
                await DisplayAlert("Permission Denied", "Can not continue, try again.", "OK");
            }
            return false;
        }
        private async void Button_ClickedAsync(object sender, EventArgs e)
        {
            if (!await CheckPermission(Permission.LocationAlways))
                return;

            //if (!await CheckPermission(Permission.Location))
            //    return;

            if (!await CheckPermission(Permission.Storage))
                return;

            viewModel.TestButtonEnabled = false;
            try
            {
                if (!await Pod.WithLotAndTid(42692, 521355))
                {
                    Pod.RadioAddress = 0x1f0e89f3;
                }

                var cts = new CancellationTokenSource();
                var progress = new MessageProgress();
                await Pod.UpdateStatus(progress, cts.Token);
                //await pod.Bolus(0.5m);
            }
            finally
            {
                viewModel.TestButtonEnabled = true;
            }
        }
    }
}
