using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Interfaces.Services;
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

    ValueTask<(PermissionStatus, bool)> GetPermissionStatusAsync(string permissionId, bool isRuntime);
    ValueTask<PermissionStatus> RequestPermissionAsync(string permissionId);
}
