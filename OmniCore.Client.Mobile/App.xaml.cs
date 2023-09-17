using OmniCore.Client.Interfaces.Services;
using OmniCore.Client.Mobile.Services;
using OmniCore.Client.Mobile.ViewModels;
using OmniCore.Client.Mobile.Views;

namespace OmniCore.Client.Mobile;
public partial class App : Application
{
    private readonly INavigationService _navigationService;
    private readonly ICoreService _coreService;

    public App(
        INavigationService navigationService,
        ICoreService coreService)
    {
        _coreService = coreService;
        InitializeComponent();
        MainPage = navigationService.NavigationPage;
    }
    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);

        window.Created += async (_, _) => { await _coreService.OnCreatedAsync(); };
        window.Activated += async (_, _) => { await _coreService.OnActivatedAsync(); };
        window.Deactivated += async (_, _) => { await _coreService.OnDeactivatedAsync(); };
        window.Stopped += async (_, _) => { await _coreService.OnStoppedAsync(); };
        window.Resumed += async (_, _) => { await _coreService.OnResumedAsync(); };
        window.Destroying += async (_, _) => { await _coreService.OnDestroyingAsync(); };
        return window;
    }

    //await _navigationService.PushAsync<PermissionsPage, PermissionsViewModel>();
}
