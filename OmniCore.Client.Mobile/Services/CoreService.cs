using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OmniCore.Client.Interfaces.Services;
using OmniCore.Client.Mobile.ViewModels;
using OmniCore.Client.Mobile.Views;
using OmniCore.Client.Model;

namespace OmniCore.Client.Mobile.Services;

public class CoreService : ICoreService
{
    private readonly INavigationService _navigationService;
    private readonly IPlatformPermissionService _permissionService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISettingsService _settingsService;

    public CoreService(INavigationService navigationService,
        IPlatformPermissionService permissionService,
        IServiceProvider serviceProvider,
        ISettingsService settingsService
        )
    {
        _navigationService = navigationService;
        _permissionService = permissionService;
        _serviceProvider = serviceProvider;
        _settingsService = settingsService;
    }
    public async Task OnCreatedAsync()
    {
        using var context = _serviceProvider.GetRequiredService<MobileDbContext>();
        await context.Database.EnsureCreatedAsync();
        await _navigationService.PushViewAsync<PermissionsPage, PermissionsViewModel>();
    }

    public Task OnActivatedAsync()
    {
        return _navigationService.AppWindowActivated();
    }

    public Task OnDeactivatedAsync()
    {
        return _navigationService.AppWindowDeactivated();
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
