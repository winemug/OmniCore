using OmniCore.Mobile.ViewModels;
using OmniCore.Model;
using OmniCore.Model.Eros;
using OmniCore.Model.Interfaces;
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
        readonly TestViewModel viewModel;

        readonly ErosPodProvider PodProvider;

        public TestPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new TestViewModel();

            PodProvider = new ErosPodProvider(new RileyLinkProvider());
            if (PodProvider.Current == null)
            {
                PodProvider.Register(42692, 521355, 0x1f0e89f3);
            }
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

            if (!await CheckPermission(Permission.Storage))
                return;

            viewModel.TestButtonEnabled = false;
            try
            {
                var cts = new CancellationTokenSource();
                var progress = new MessageProgress();
                await PodProvider.Current.UpdateStatus(progress, cts.Token);
            }
            finally
            {
                viewModel.TestButtonEnabled = true;
            }
        }
    }
}
