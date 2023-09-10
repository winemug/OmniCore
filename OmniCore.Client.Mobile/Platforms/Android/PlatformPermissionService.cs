using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.OS;
using AndroidX.Core.App;
using OmniCore.Client.Interfaces.Services;

namespace OmniCore.Client.Mobile.Services
{
    public class PlatformPermissionService : IPlatformPermissionService
    {
        public async Task<bool> RequiresBluetoothPermissionAsync()
        {
            return await Permissions.CheckStatusAsync<BluetoothPermission>() == PermissionStatus.Granted;
        }

        public async Task<bool> RequestBluetoothPermissionAsync()
        {
            return await Permissions.RequestAsync<BluetoothPermission>() == PermissionStatus.Granted;
        }

        public async Task<(PermissionStatus, bool)> GetPermissionStatusAsync(string permissionId, bool isRuntime)
        {
            MyPermission.IsRuntime = isRuntime;
            MyPermission.PermissionId = permissionId;
            var status = await Permissions.CheckStatusAsync<MyPermission>();
            var shouldShowRationale = Permissions.ShouldShowRationale<MyPermission>();
            return (status, shouldShowRationale);
        }

        public async Task<PermissionStatus> RequestPermissionAsync(string permissionId)
        {
            MyPermission.IsRuntime = true;
            MyPermission.PermissionId = permissionId;
            return await Permissions.RequestAsync<MyPermission>();
        }
    }

    public class BluetoothPermission : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            (Build.VERSION.SdkInt <= BuildVersionCodes.R) ?
            new (string androidPermission, bool isRuntime)[]
            {
                (global::Android.Manifest.Permission.Bluetooth, false),
                (global::Android.Manifest.Permission.BluetoothAdmin, false),
                (global::Android.Manifest.Permission.AccessBackgroundLocation, true),
                (global::Android.Manifest.Permission.AccessFineLocation, true)
            } :
            new (string androidPermission, bool isRuntime)[]
            {
                (global::Android.Manifest.Permission.BluetoothConnect, true),
                (global::Android.Manifest.Permission.BluetoothScan, true)
            };
    }

    public class MyPermission : Permissions.BasePlatformPermission
    {
        public static string PermissionId { get; set; }
        public static bool IsRuntime { get; set; }

        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            new (string androidPermission, bool isRuntime)[]
            {
                (PermissionId, IsRuntime),
            };
    }

}
