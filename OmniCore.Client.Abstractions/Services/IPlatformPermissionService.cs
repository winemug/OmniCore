using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Abstractions.Services;
public interface IPlatformPermissionService
{
    ValueTask<bool> RequiresBluetoothPermissionAsync();
    ValueTask<bool> RequestBluetoothPermissionAsync();

    ValueTask<bool> RequiresForegroundPermissionAsync();
    ValueTask<bool> RequestForegroundPermissionAsync();

    ValueTask<bool> IsBatteryOptimizedAsync();
    ValueTask<bool> TryExemptFromBatteryOptimization();

    ValueTask<bool> IsBackgroundDataRestrictedAsync();
    ValueTask<bool> TryExemptFromBackgroundDataRestriction();

    ValueTask<(bool, bool)> GetPermissionStatusAsync(string permissionId, bool isRuntime);
    ValueTask<bool> RequestPermissionAsync(string permissionId);
}
