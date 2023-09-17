using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.Interfaces.Services;
using OmniCore.Client.Mobile.ViewModels;
using OmniCore.Client.Mobile.Views;

namespace OmniCore.Client.Mobile.Services;

public class CoreService : ICoreService
{
    private readonly INavigationService _navigationService;

    public CoreService(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }
    public ValueTask OnCreatedAsync()
    {
        return _navigationService.PushAsync<PermissionsPage, PermissionsViewModel>();
    }

    public ValueTask OnActivatedAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask OnDeactivatedAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask OnStoppedAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask OnResumedAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask OnDestroyingAsync()
    {
        return ValueTask.CompletedTask;
    }
}
