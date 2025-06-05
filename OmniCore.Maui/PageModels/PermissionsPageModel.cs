using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OmniCore.Maui.Interfaces;
using OmniCore.Maui.Models;

namespace OmniCore.Maui.PageModels;

public partial class PermissionsPageModel(IPlatformService platformService) : PageModel
{
    [ObservableProperty] List<DynamicPermissionModel> _dynamicPermissions =
    [
        new DynamicPermissionModel(permissionName: "android.permission.INTERNET", true),
        new DynamicPermissionModel(permissionName: "android.permission.ACCESS_NETWORK_STATE", true),
        
        new DynamicPermissionModel(permissionName: "android.permission.ACCESS_COARSE_LOCATION", true),
        new DynamicPermissionModel(permissionName: "android.permission.ACCESS_FINE_LOCATION", true),
        
        new DynamicPermissionModel(permissionName: "android.permission.BLUETOOTH", true),
        new DynamicPermissionModel(permissionName: "android.permission.BLUETOOTH_ADMIN", true),
        new DynamicPermissionModel(permissionName: "android.permission.BLUETOOTH_CONNECT", true),
        new DynamicPermissionModel(permissionName: "android.permission.BLUETOOTH_SCAN", true),
        new DynamicPermissionModel(permissionName: "android.permission.BLUETOOTH_ADVERTISE", true),
        
        new DynamicPermissionModel(permissionName: "android.permission.POST_NOTIFICATIONS", true),
        new DynamicPermissionModel(permissionName: "android.permission.FOREGROUND_SERVICE", true),
        new DynamicPermissionModel(permissionName: "android.permission.FOREGROUND_SERVICE_SYSTEM_EXEMPTED", true),
        new DynamicPermissionModel(permissionName: "android.permission.USE_EXACT_ALARM", true),
    ];

    protected override async Task Appearing()
    {
        foreach (var permission in DynamicPermissions)
        {
            await permission.UpdateStatusAsync();
        }
    }

    [RelayCommand]
    private async Task RequestAllPermissions()
    {
        foreach (var permission in DynamicPermissions)
        {
            await permission.RequestAndUpdateStatusAsync();
        }
    }
}