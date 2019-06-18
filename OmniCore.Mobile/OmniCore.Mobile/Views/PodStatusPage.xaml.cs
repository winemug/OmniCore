using OmniCore.Mobile.Interfaces;
using OmniCore.Mobile.ViewModels;
using OmniCore.Model;
using OmniCore.Model.Interfaces;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PodStatusPage : ContentPage
    {
        PodStatusViewModel viewModel;

        public PodStatusPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new PodStatusViewModel();
        }

        private async void Update_Button_Clicked(object sender, EventArgs e)
        {
            var podManager = App.Instance.PodProvider.PodManager;
            using (var conversation = await podManager.StartConversation())
            {
                await podManager.UpdateStatus(conversation);
            }
        }

        private bool ensureCalled = false;
        private async void ContentPage_Appearing(object sender, EventArgs e)
        {
            if (!ensureCalled)
            {
                ensureCalled = true;
                await EnsurePermissions();
            }
            viewModel.StartUpdateTimer();
        }

        private void ContentPage_Disappearing(object sender, EventArgs e)
        {
            viewModel.StopUpdateTimer();
        }

        private async Task EnsurePermissions()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.LocationAlways);
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Missing Permissions", "You have to grant the location permission to this application in order to be able to connect to bluetooth devices.", "OK");
                    var request = await CrossPermissions.Current.RequestPermissionsAsync(Permission.LocationAlways);
                    if (request[Permission.LocationAlways] != PermissionStatus.Granted)
                    {
                        await DisplayAlert("Missing Permissions", "This application cannot run without the necessary permissions.", "OK");
                        DependencyService.Get<IOmniCoreApplication>().Exit();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Missing Permissions", "Error while querying / acquiring permissions", "OK");
            }
        }
    }
}