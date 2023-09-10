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

namespace OmniCore.Client.Mobile.ViewModels;

public partial class PermissionsViewModel : ObservableObject
{
    private readonly IPlatformPermissionService _platformPermissionService;

    [ObservableProperty] private string? _permissionStatusResult;
    [ObservableProperty] private string? _permissionRequestResult;

    [ObservableProperty] private bool _bluetooth;
    [ObservableProperty] private bool _foreground;
    [ObservableProperty] private bool _battery;
    [ObservableProperty] private bool _data;

    public PermissionsViewModel(IPlatformPermissionService platformPermissionService)
    {
        _platformPermissionService = platformPermissionService;
        UpdateChecks();
    }

    [RelayCommand]
    private async void UpdateChecks()
    {
        Bluetooth = await _platformPermissionService.RequiresBluetoothPermissionAsync();
        Foreground = await _platformPermissionService.RequiresForegroundPermissionAsync();
        Battery = await _platformPermissionService.IsBatteryOptimizedAsync();
        Data = await _platformPermissionService.IsBackgroundDataRestrictedAsync();
    }

    [RelayCommand]
    private async void RequestBluetoothPermission()
    {
        Bluetooth = !await _platformPermissionService.RequestBluetoothPermissionAsync();
    }

    [RelayCommand]
    private async void RequestForegroundPermission()
    {
        Foreground = !await _platformPermissionService.RequestForegroundPermissionAsync();
    }

    [RelayCommand]
    private async void RequestBatteryOptimizationExemption()
    {
        Battery = await _platformPermissionService.TryExemptFromBatteryOptimization();
    }

    [RelayCommand]
    private async void RequestBackgroundDataExemption()
    {
        Data = await _platformPermissionService.TryExemptFromBackgroundDataRestriction();
    }

    [RelayCommand]
    private async void CheckPermissions()
    {

        var sb = new StringBuilder();
        foreach (var (permissionName, isRuntime) in new[]
                 {
                     ("android.permission.BLUETOOTH", false),
                     ("android.permission.BLUETOOTH_ADMIN", false),
                     ("android.permission.BLUETOOTH_ADVERTISE", false),
                     ("android.permission.BLUETOOTH_CONNECT", true),
                     ("android.permission.BLUETOOTH_SCAN", true)
                 })
        {
            var resultText = await GetPermissionStatusAsync(permissionName, isRuntime);
            sb.Append(permissionName).Append(" ").AppendLine(resultText);
        }

        PermissionStatusResult = sb.ToString();
    }

    [RelayCommand]
    private async void RequestPermissions()
    {
        var sb = new StringBuilder();
        foreach (var permissionName in new[]
                 {
                     "android.permission.BLUETOOTH",
                     "android.permission.BLUETOOTH_ADMIN",
                     "android.permission.BLUETOOTH_ADVERTISE",
                     "android.permission.BLUETOOTH_CONNECT",
                     "android.permission.BLUETOOTH_PRIVILEGED",
                     "android.permission.BLUETOOTH_SCAN",
                 })
        {
            sb.Append(permissionName).Append(" ");
            try
            {
                var r = await _platformPermissionService.RequestPermissionAsync(permissionName);
                sb.AppendLine(r.ToString());
            }
            catch (Exception e)
            {
                sb.AppendLine(e.Message);
            }
        }

        PermissionRequestResult = sb.ToString();

    }

    private async Task<string> GetPermissionStatusAsync(string permissionName, bool isRuntime)
    {
        try
        {
            var r = await _platformPermissionService.GetPermissionStatusAsync(permissionName, isRuntime);
            return $"{r.Item1.ToString()} RationaleReq: {r.Item2}";
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

}
