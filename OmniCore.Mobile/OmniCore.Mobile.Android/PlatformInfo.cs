using System;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using OmniCore.Services.Interfaces;
using Xamarin.Essentials;

namespace OmniCore.Mobile.Droid
{
    public class PlatformInfo : IPlatformInfo
    {
        private readonly Activity _activity;
        private readonly Context _context;
        private int _requestCode;

        private TaskCompletionSource<bool> _tcsPermissionsResult;

        public PlatformInfo(Context context, Activity activity)
        {
            _context = context;
            _activity = activity;
            Platform = "Android";
            HardwareVersion = "1.0.0";
            SoftwareVersion = "1.0.0";
            OsVersion = "10";
        }

        public string SoftwareVersion { get; }
        public string HardwareVersion { get; }
        public string OsVersion { get; }
        public string Platform { get; }

        public bool IsExemptFromBatteryOptimizations => !IsBatteryOptimized();

        public bool HasAllPermissions =>
            //                    HasPermission(Manifest.Permission.Bluetooth) &&
            //                    HasPermission(Manifest.Permission.BluetoothAdmin) &&
            HasPermission(Manifest.Permission.AccessFineLocation) &&
            HasPermission(Manifest.Permission.AccessBackgroundLocation) &&
            HasPermission(Manifest.Permission.WriteExternalStorage) &&
            HasPermission(Manifest.Permission.ReadExternalStorage) &&
            HasPermission(Manifest.Permission.AccessNetworkState) &&
            HasPermission(Manifest.Permission.Internet);

        public async Task<bool> RequestMissingPermissions()
        {
            if (!await RequestIfMissing(new[]
                {
                    Manifest.Permission.AccessNetworkState,
                    Manifest.Permission.Internet
                }))
                return false;

            if (!await RequestIfMissing(new[]
                {
                    Manifest.Permission.Bluetooth,
                    Manifest.Permission.BluetoothAdmin,
                    Manifest.Permission.AccessFineLocation
                }))
                return false;

            if (!await RequestIfMissing(new[]
                {
                    Manifest.Permission.AccessBackgroundLocation
                }))
                return false;

            if (!await RequestIfMissing(new[]
                {
                    Manifest.Permission.ReadExternalStorage,
                    Manifest.Permission.WriteExternalStorage
                }))
                return false;

            return true;
        }

        public void OpenBatteryOptimizationSettings()
        {
            var intent = new Intent();
            intent.SetAction(Settings.ActionIgnoreBatteryOptimizationSettings);
            _context.StartActivity(intent);
        }

        private async Task<bool> RequestIfMissing(string[] permissions)
        {
            var missing = false;
            foreach (var permission in permissions)
                if (!HasPermission(permission))
                {
                    missing = true;
                    break;
                }

            if (missing)
            {
                _tcsPermissionsResult = new TaskCompletionSource<bool>();
                _requestCode = new Random().Next(1, 254);
                ActivityCompat.RequestPermissions(_activity, permissions, _requestCode);

                if (!await _tcsPermissionsResult.Task.ConfigureAwait(false))
                    return false;
            }

            return true;
        }

        public void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode != _requestCode)
                return;

            _tcsPermissionsResult.SetResult(grantResults.All(r => r == Permission.Granted));
        }

        private bool HasPermission(string permissionName)
        {
            return ContextCompat.CheckSelfPermission(_context, permissionName) == (int)Permission.Granted;
        }

        private bool IsBatteryOptimized()
        {
            var pm = (PowerManager)_context.GetSystemService(Context.PowerService);
            return !pm.IsIgnoringBatteryOptimizations(AppInfo.PackageName);
        }
    }
}