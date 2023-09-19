using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OmniCore.Client.Interfaces.Services;
using OmniCore.Client.Mobile.Services;
using OmniCore.Client.Mobile.Views;

namespace OmniCore.Client.Mobile.ViewModels;

public partial class PermissionsViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IPlatformPermissionService _platformPermissionService;

    [ObservableProperty] private string? _permissionStatusResult;
    [ObservableProperty] private string? _permissionRequestResult;

    [ObservableProperty] private bool _bluetoothPermissionRequired;
    [ObservableProperty] private bool _foregroundPermissionRequired;
    [ObservableProperty] private bool _requestBatteryOptimizationExemptionRequired;
    [ObservableProperty] private bool _backgroundDataExemptionRequired;

    public PermissionsViewModel(
        INavigationService navigationService,
        IPlatformPermissionService platformPermissionService)
    {
        _navigationService = navigationService;
        _platformPermissionService = platformPermissionService;
    }
    private async Task ReadPermissionsAsync()
    {
        BluetoothPermissionRequired = await _platformPermissionService.RequiresBluetoothPermissionAsync();
        ForegroundPermissionRequired = await _platformPermissionService.RequiresForegroundPermissionAsync();
        RequestBatteryOptimizationExemptionRequired = await _platformPermissionService.IsBatteryOptimizedAsync();
        BackgroundDataExemptionRequired = await _platformPermissionService.IsBackgroundDataRestrictedAsync();
    }

    public override Task OnNavigatingTo()
    {
        return ReadPermissionsAsync();
    }

    [RelayCommand]
    private async Task RequestBluetoothPermission()
    {
        await _platformPermissionService.RequestBluetoothPermissionAsync();
        await ReadPermissionsAsync();
    }

    [RelayCommand]
    private async Task RequestForegroundPermission()
    {
        await _platformPermissionService.RequestForegroundPermissionAsync();
        await ReadPermissionsAsync();
    }

    [RelayCommand]
    private async Task RequestBatteryOptimizationExemption()
    {
        await _platformPermissionService.TryExemptFromBatteryOptimization();
        await ReadPermissionsAsync();
    }

    [RelayCommand]
    private async Task RequestBackgroundDataExemption()
    {
        await _platformPermissionService.TryExemptFromBackgroundDataRestriction();
        await ReadPermissionsAsync();
    }

    [RelayCommand]
    private async Task Continue()
    {
        await _navigationService.PushAsync<AccountLoginPage, AccountLoginViewModel, int>(3);
    }

}
