using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Interfaces.Services;
public interface IPlatformPermissionService
{
    Task<bool> RequiresBluetoothPermissionAsync();
    Task<bool> RequestBluetoothPermissionAsync();
    Task<(PermissionStatus, bool)> GetPermissionStatusAsync(string permissionId, bool isRuntime);
    Task<PermissionStatus> RequestPermissionAsync(string permissionId);
}
