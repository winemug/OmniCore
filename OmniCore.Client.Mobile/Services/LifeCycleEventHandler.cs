using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.Interfaces.Services;
using OmniCore.Client.Mobile.ViewModels;
using OmniCore.Client.Mobile.Views;

namespace OmniCore.Client.Mobile.Services;
public class LifeCycleEventHandler : ILifeCycleEventHandler
{
    private readonly INavigationService _navigationService;

    public LifeCycleEventHandler(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public async ValueTask OnCreatedAsync()
    {
        await _navigationService.PushAsync<PermissionsPage, PermissionsViewModel>();
    }
    public async ValueTask OnDestroyingAsync()
    {
    }

    public async ValueTask OnStoppedAsync()
    {
    }

    public async ValueTask OnResumedAsync()
    {
    }

    public async ValueTask OnActivatedAsync()
    {
    }

    public async ValueTask OnDeactivatedAsync()
    {
    }

}
