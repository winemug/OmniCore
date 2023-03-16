using Android.Content;
using Android.OS;
using OmniCore.Services.Interfaces.Platform;
using static Microsoft.Maui.ApplicationModel.Permissions;
using Debug = System.Diagnostics.Debug;

namespace OmniCore.Maui
{
    public class AndroidPlatformInfo : IPlatformInfo
    {
        public AndroidPlatformInfo()
        {
            Platform = "Android";
            HardwareVersion = "1.0.0";
            SoftwareVersion = "1.0.0";
            OsVersion = "10";
        }

        public string SoftwareVersion { get; }
        public string HardwareVersion { get; }
        public string OsVersion { get; }
        public string Platform { get; }

        public async Task<bool> VerifyPermissions()
        {
            if (await CheckAndRequest<BluetoothPermissions>() != PermissionStatus.Granted)
                return false;
            if (await CheckAndRequest<ForegroundPermissions>() != PermissionStatus.Granted)
                return false;
            
            // if (!await RequestIfMissing(new[]
            //     {
            //         Manifest.Permission.AccessNetworkState,
            //         Manifest.Permission.Internet
            //     }))
            //     return false;
            //
            // if (!await RequestIfMissing(new[]
            //     {
            //         Manifest.Permission.Bluetooth,
            //         Manifest.Permission.BluetoothAdmin,
            //         Manifest.Permission.BluetoothConnect,
            //         Manifest.Permission.BluetoothScan,
            //     }))
            //     return false;
            //
            // if (!await RequestIfMissing(new []
            //     {
            //         Manifest.Permission.ForegroundService,
            //     }))
            //     return false;
            //
            // if (!await RequestIfMissing(new[]
            //     {
            //         Manifest.Permission.ReadExternalStorage,
            //         Manifest.Permission.WriteExternalStorage
            //     }))
            //     return false;
            //
            // if (!await RequestIfMissing(new[]
            //     {
            //         Manifest.Permission.AccessFineLocation,
            //         Manifest.Permission.AccessBackgroundLocation
            //     }))
            //     return false;
            //
            //
            var pm = (PowerManager)MauiApplication.Current.GetSystemService(Context.PowerService);
            if (pm.IsIgnoringBatteryOptimizations(MauiApplication.Current.PackageName))
            {
                return true;
            }

            AppInfo.Current.ShowSettingsUI();

            // var intent = new Intent();
            // intent.SetAction(Settings.ActionIgnoreBatteryOptimizationSettings);
            // MauiApplication.Current.StartActivity(intent);
            return false;
        }

        private async Task<PermissionStatus> CheckAndRequest<T>() where T : BasePermission, new()
        { 
            Debug.WriteLine($"Checking permission {typeof(T)}");
            var status = await Permissions.CheckStatusAsync<T>();
            Debug.WriteLine($"Permission status: {status}");
            if (status != PermissionStatus.Granted)
            {
                Debug.WriteLine($"Requesting permission");
                status = await Permissions.RequestAsync<T>();
                Debug.WriteLine($"Request result: {status}");
            }

            return status;
        }
    }
    public class BluetoothPermissions : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            new List<(string androidPermission, bool isRuntime)>
            {
                (global::Android.Manifest.Permission.Bluetooth, false),
                (global::Android.Manifest.Permission.BluetoothAdmin, false),
                (global::Android.Manifest.Permission.BluetoothConnect, true),
                (global::Android.Manifest.Permission.BluetoothScan, true)
            }.ToArray();
    }
    
    public class ForegroundPermissions : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            new List<(string androidPermission, bool isRuntime)>
            {
                (global::Android.Manifest.Permission.ForegroundService, false),
            }.ToArray();
    }
}