using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Provider;
using AndroidX.Core.App;
using OmniCore.Client.Interfaces.Services;
using static Microsoft.Maui.ApplicationModel.Platform;
using Intent = Android.Content.Intent;
using Uri = Android.Net.Uri;

namespace OmniCore.Client.Mobile.Services
{
    public class PlatformPermissionService : IPlatformPermissionService
    {
        public async ValueTask<bool> RequiresBluetoothPermissionAsync()
        {
            return await Permissions.CheckStatusAsync<BluetoothPermission>() != PermissionStatus.Granted;
        }

        public async ValueTask<bool> RequestBluetoothPermissionAsync()
        {
            return await Permissions.RequestAsync<BluetoothPermission>() == PermissionStatus.Granted;
        }

        public async ValueTask<bool> RequiresForegroundPermissionAsync()
        {
            return await Permissions.CheckStatusAsync<ForegroundPermission>() != PermissionStatus.Granted;
        }

        public async ValueTask<bool> RequestForegroundPermissionAsync()
        {
            return await Permissions.RequestAsync<ForegroundPermission>() == PermissionStatus.Granted;
        }

        public async ValueTask<bool> IsBatteryOptimizedAsync()
        {
            var pm = (PowerManager)MauiApplication.Current.GetSystemService(Context.PowerService);
            return !pm.IsIgnoringBatteryOptimizations(MauiApplication.Current.PackageName);
        }

        public async ValueTask<bool> TryExemptFromBatteryOptimization()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                Platform.CurrentActivity?.StartActivity(new Intent(Settings.ActionRequestIgnoreBatteryOptimizations, Uri.Parse($"package:{MauiApplication.Current.PackageName}")));
            }
            else
            {
                Platform.CurrentActivity?.StartActivity(new Intent(Settings.ActionIgnoreBatteryOptimizationSettings));
            }
            return true;
        }

        public async ValueTask<bool> IsBackgroundDataRestrictedAsync()
        {
            var cm = (ConnectivityManager)MauiApplication.Current.GetSystemService(Context.ConnectivityService);
            if (!cm.IsActiveNetworkMetered)
                return false;

            return cm.RestrictBackgroundStatus switch
            {
                RestrictBackgroundStatus.Disabled or RestrictBackgroundStatus.Whitelisted => false,
                _ => true,
            };
        }

        public async ValueTask<bool> TryExemptFromBackgroundDataRestriction()
        {

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                MauiApplication.Current.StartActivity(new Intent(Settings.ActionIgnoreBackgroundDataRestrictionsSettings, Uri.Parse($"package:{MauiApplication.Current.PackageName}")));
            }
            else
            {
                MauiApplication.Current.StartActivity(new Intent(Settings.ActionIgnoreBackgroundDataRestrictionsSettings));
            }
            return true;
        }

        public async ValueTask<(PermissionStatus, bool)> GetPermissionStatusAsync(string permissionId, bool isRuntime)
        {
            MyPermission.IsRuntime = isRuntime;
            MyPermission.PermissionId = permissionId;
            var status = await Permissions.CheckStatusAsync<MyPermission>();
            var shouldShowRationale = Permissions.ShouldShowRationale<MyPermission>();
            return (status, shouldShowRationale);
        }

        public async ValueTask<PermissionStatus> RequestPermissionAsync(string permissionId)
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
                (global::Android.Manifest.Permission.AccessFineLocation, true),
            } :
            new (string androidPermission, bool isRuntime)[]
            {
                (global::Android.Manifest.Permission.BluetoothConnect, true),
                (global::Android.Manifest.Permission.BluetoothScan, true),
                (global::Android.Manifest.Permission.AccessFineLocation, true),
            };
    }

    public class ForegroundPermission: Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            new (string androidPermission, bool isRuntime)[]
                {
                    (global::Android.Manifest.Permission.ForegroundService, true),
                    (global::Android.Manifest.Permission.PostNotifications, true),
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
