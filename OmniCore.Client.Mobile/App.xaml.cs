using OmniCore.Client.Mobile.Services;
using OmniCore.Client.Mobile.ViewModels;
using OmniCore.Client.Mobile.Views;

namespace OmniCore.Client.Mobile
{
    public partial class App : Application
    {

        public App(NavigationService navigationService)
        {
            InitializeComponent();

            MainPage = navigationService.NavigationPage;
            navigationService.PushAsync<PermissionsPage, PermissionsViewModel>();
        }
    }
}