using Android.Content;
using Android.OS;
using Android.Provider;
using OmniCore.Common.Platform;
using static Microsoft.Maui.ApplicationModel.Permissions;
using Debug = System.Diagnostics.Debug;

namespace OmniCore.Maui.Services
{
    public class PlatformInfo : IPlatformInfo
    {
        //public partial string SoftwareVersion { get; }
        //public partial string HardwareVersion { get; }
        //public partial string OsVersion { get; }
        //public partial string Platform { get; }

        public async Task<bool> VerifyPermissions(bool silent)
        {
            var status = await CheckAndRequest<BluetoothPermissions>(silent);
            if (status != PermissionStatus.Granted)
                return false;

            status = await CheckAndRequest<ForegroundPermissions>(silent);
            if (status != PermissionStatus.Granted)
                return false;

            status = await CheckAndRequest<NotificationPermissions>(silent);
            if (status != PermissionStatus.Granted)
                return false;

            var pm = (PowerManager)MauiApplication.Current.GetSystemService(Context.PowerService);
            if (pm.IsIgnoringBatteryOptimizations(MauiApplication.Current.PackageName))
            {
                return true;
            }

            if (silent)
                return false;

            var intent = new Intent(Settings.ActionIgnoreBatteryOptimizationSettings);
            Platform.CurrentActivity.StartActivity(intent);
            return false;
        }

        public string GetUserName()
        {
            //TODO:
            return "";
        }

        public string GetVersion()
        {
            return "1.0.0.0";
        }

        //public PlatformInfo()
        //{
        //    Platform = "Android";
        //    HardwareVersion = "1.0.0";
        //    SoftwareVersion = "1.0.0";
        //    OsVersion = "10";
        //}

        private async Task<PermissionStatus> CheckAndRequest<T>(bool silent) where T : BasePermission, new()
        { 
            //Debug.WriteLine($"----Permission Check Start-----");
            //Debug.WriteLine($"Checking permission {typeof(T).Name}");
            var status = await Permissions.CheckStatusAsync<T>();
            Debug.WriteLine($"Permission {typeof(T).Name}: {status}");
            if (status != PermissionStatus.Granted && !silent)
            {
                //Debug.WriteLine($"Requesting permission {typeof(T).Name}");
                status = await Permissions.RequestAsync<T>();
                Debug.WriteLine($"Request {typeof(T).Name} result: {status}");
            }
            //Debug.WriteLine($"----Permission Check End-----");

            return status;
        }
    }
    public class BluetoothPermissions : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            (Build.VERSION.SdkInt <= BuildVersionCodes.R)
                ? new List<(string androidPermission, bool isRuntime)>
                {
                    (global::Android.Manifest.Permission.Bluetooth, false),
                    (global::Android.Manifest.Permission.BluetoothAdmin, false),
                    (global::Android.Manifest.Permission.AccessBackgroundLocation, true),
                    (global::Android.Manifest.Permission.AccessFineLocation, true),
                }.ToArray()
                : new List<(string androidPermission, bool isRuntime)>
                {
                    (global::Android.Manifest.Permission.BluetoothConnect, true),
                    (global::Android.Manifest.Permission.BluetoothScan, true)
                }.ToArray();
    }
    
    public class ForegroundPermissions : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            new List<(string androidPermission, bool isRuntime)>
            {
                (global::Android.Manifest.Permission.ForegroundService, true),
            }.ToArray();
    }
    
    public class NotificationPermissions : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            new List<(string androidPermission, bool isRuntime)>
            {
                (global::Android.Manifest.Permission.PostNotifications, true),
            }.ToArray();
    }
}