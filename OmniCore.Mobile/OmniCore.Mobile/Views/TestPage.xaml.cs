using OmniCore.Mobile.ViewModels;
using OmniCore.Model;
using OmniCore.Radio.RileyLink;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Mobile.Views
{
    public partial class TestPage : ContentPage
    {
        TestViewModel viewModel;

        public TestPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new TestViewModel();
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

            if (!await CheckPermission(Permission.Location))
                return;

            if (!await CheckPermission(Permission.Storage))
                return;

            viewModel.TestButtonEnabled = false;
            try
            {
                var rlp = new RileyLinkProvider();
                var pod = new Pod(rlp)
                {
                    id_lot = 44538, id_t = 1181076, radio_address = 0x34ff1d53
                };
                await pod.UpdateStatus();
                await pod.Bolus(0.5m);
            }
            finally
            {
                viewModel.TestButtonEnabled = true;
            }
        }
    }
}
