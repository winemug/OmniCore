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
    private readonly IPlatformPermissionService _platformPermissionService;
    private readonly IPlatformForegroundService _platformForegroundService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ISettingsService _settingsService;
    private volatile int _foregroundContextCount = 0; 
    
    public CoreService(INavigationService navigationService,
        IPlatformPermissionService platformPermissionService,
        IPlatformForegroundService platformForegroundService,
        IServiceScopeFactory serviceScopeFactory,
        ISettingsService settingsService
        )
    {
        _navigationService = navigationService;
        _platformPermissionService = platformPermissionService;
        _platformForegroundService = platformForegroundService;
        _serviceScopeFactory = serviceScopeFactory;
        _settingsService = settingsService;
    }
    
    public async Task InteractiveStartAsync()
    {
        var scope = _serviceScopeFactory.CreateScope();
        using (var context = scope.ServiceProvider.GetRequiredService<MobileDbContext>())
        {
            await context.Database.EnsureCreatedAsync();
        }

        var permissionsRequired = await _platformPermissionService.RequiresBluetoothPermissionAsync();
        permissionsRequired |= await _platformPermissionService.RequiresForegroundPermissionAsync();
        permissionsRequired |= await _platformPermissionService.IsBatteryOptimizedAsync();
        permissionsRequired |= await _platformPermissionService.IsBackgroundDataRestrictedAsync();

        if (permissionsRequired)
        {
            await _navigationService.PushViewAsync<PermissionsPage, PermissionsViewModel>();
            return;
        }

        await _navigationService.PushViewAsync<AccountLoginPage, AccountLoginViewModel>();
    }

    public IDisposable ForegroundActivityContext()
    {
        lock (this)
        {
            if (_foregroundContextCount <= 0)
            {
                _platformForegroundService.StartForeground();
            }
        }
        Interlocked.Increment(ref _foregroundContextCount);
        return new ForegroundActivityContext(DisposeActivityContext);
    }

    private void DisposeActivityContext()
    {
        lock (this)
        {
            Interlocked.Decrement(ref _foregroundContextCount);
            if (_foregroundContextCount <= 0)
            {
                _platformForegroundService.StopForeground();
            }
        }
    }
    
    public Task OnCreatedAsync()
    {
        return InteractiveStartAsync();
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

public class ForegroundActivityContext : IDisposable
{
    private Action? _disposeAction;

    public ForegroundActivityContext(Action disposeAction)
    {
        _disposeAction = disposeAction;
    }
    public void Dispose()
    {
        if (_disposeAction != null)
        {
            _disposeAction.Invoke();
            _disposeAction = null;
        }
    }
}