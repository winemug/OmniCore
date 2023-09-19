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
    private readonly IPlatformPermissionService _permissionService;

    public CoreService(INavigationService navigationService,
        IPlatformPermissionService permissionService)
    {
        _navigationService = navigationService;
        _permissionService = permissionService;
    }
    public Task OnCreatedAsync()
    {
        return _navigationService.PushAsync<PermissionsPage, PermissionsViewModel>();
    }

    public Task OnActivatedAsync()
    {
        return Task.CompletedTask;
    }

    public Task OnDeactivatedAsync()
    {
        return Task.CompletedTask;
    }

    public Task OnStoppedAsync()
    {
        return Task.CompletedTask;
    }

    public Task OnResumedAsync()
    {
        return Task.CompletedTask;
    }

    public Task OnDestroyingAsync()
    {
        return Task.CompletedTask;
    }
}
