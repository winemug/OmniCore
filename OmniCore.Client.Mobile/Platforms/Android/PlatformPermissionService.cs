using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AndroidX.Core.App;
using OmniCore.Client.Interfaces.Services;

namespace OmniCore.Client.Mobile.Services
{
    public class PlatformPermissionService : IPlatformPermissionService
    {
        public async Task<PermissionStatus> GetPermissionStatusAsync(string permissionId)
        {
            return await Permissions.CheckStatusAsync<MyPermission>();
        }
    }

    public class MyPermission : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            new (string androidPermission, bool isRuntime)[]
            {
                (global::Android.Manifest.Permission.Bluetooth, false),
                //(global::Android.Manifest.Permission.BluetoothAdmin, false),
                //(global::Android.Manifest.Permission.AccessBackgroundLocation, true),
                //(global::Android.Manifest.Permission.AccessFineLocation, true)
                // (global::Android.Manifest.Permission.BluetoothConnect, false),
                //(global::Android.Manifest.Permission.BluetoothScan, false)
            };
    }

}
