using OmniCore.Common.Core;
using OmniCore.Common.Platform;
using OmniCore.Maui.Services;
using OmniCore.Maui.ViewModels;
using OmniCore.Maui.Views;

namespace OmniCore.Maui;

public partial class App : Application
{
    public App(
        IAppConfiguration appConfiguration,
        NavigationService navigationService)
    {
        InitializeComponent();
        MainPage = navigationService.NavigationPage;
        
        if (!appConfiguration.AccountVerified || appConfiguration.ClientAuthorization == null)
            navigationService.PushAsync<AccountLoginPage, AccountLoginViewModel>().GetAwaiter().GetResult();
    }
}