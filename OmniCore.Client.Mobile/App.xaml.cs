using OmniCore.Client.Interfaces.Services;
using OmniCore.Client.Mobile.Services;
using OmniCore.Client.Mobile.ViewModels;
using OmniCore.Client.Mobile.Views;

namespace OmniCore.Client.Mobile;
public partial class App : Application
{
    private readonly INavigationService _navigationService;
    private readonly ILifeCycleEventHandler _lifecycleEventHandler;

    public App(INavigationService navigationService,
        ILifeCycleEventHandler lifecycleEventHandler)
    {
        _navigationService = navigationService;
        _lifecycleEventHandler = lifecycleEventHandler;
        InitializeComponent();
        MainPage = navigationService.NavigationPage;
    }
    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);

        window.Created += async (_, _) => { await _lifecycleEventHandler.OnCreatedAsync(); };
        window.Activated += async (_, _) => { await _lifecycleEventHandler.OnActivatedAsync(); };
        window.Deactivated += async (_, _) => { await _lifecycleEventHandler.OnDeactivatedAsync(); };
        window.Stopped += async (_, _) => { await _lifecycleEventHandler.OnStoppedAsync(); };
        window.Resumed += async (_, _) => { await _lifecycleEventHandler.OnResumedAsync(); };
        window.Destroying += async (_, _) => { await _lifecycleEventHandler.OnDestroyingAsync(); };
        return window;
    }
}
