using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Client.Droid.Platform
{
    public class PlatformConfiguration : IPlatformConfiguration
    {
        private const string SharedPreferenceName = "OmniCore";
        private const string KeyTermsAccepted = "TermsAccepted";
        private const string KeyServiceEnabled = "ServiceEnabled";

        private const string WriteExternalStorage = "android.permission.WRITE_EXTERNAL_STORAGE";
        private const string ReadExternalStorage = "android.permission.READ_EXTERNAL_STORAGE";

        private const string Bluetooth = "android.permission.BLUETOOTH";
        private const string BluetoothAdmin = "android.permission.BLUETOOTH_ADMIN";
        private const string BluetoothPrivileged = "android.permission.BLUETOOTH_PRIVILEGED";
        private const string AccessCoarseLocation = "android.permission.ACCESS_COARSE_LOCATION";
        
        private readonly IClientFunctions ClientFunctions;
        public PlatformConfiguration(IClientFunctions clientFunctions)
        {
            ClientFunctions = clientFunctions;
        }
        public bool ServiceEnabled
        {
            get => ReadBool(KeyServiceEnabled, false);
            set => WriteKey(KeyServiceEnabled, value);
        }

        public bool TermsAccepted
        {
            get => ReadBool(KeyTermsAccepted, false);
            set => WriteKey(KeyTermsAccepted, value);
        }

        public async Task<bool> BluetoothPermissionGranted()
        {
            return await HasAllPermissions(Bluetooth,
                BluetoothAdmin, BluetoothPrivileged, AccessCoarseLocation);
        }

        public async Task<bool> StoragePermissionGranted()
        {
            return await HasAllPermissions(ReadExternalStorage,
                WriteExternalStorage);
        }

        public async Task<bool> RequestBluetoothPermission()
        {
            return await ClientFunctions.RequestPermissions(Bluetooth,
                BluetoothAdmin, BluetoothPrivileged, AccessCoarseLocation)
                .All(pr => pr.IsGranted)
                .ToTask();
        }

        public async Task<bool> RequestStoragePermission()
        {
            return await ClientFunctions.RequestPermissions(ReadExternalStorage,
                    WriteExternalStorage)
                .All(pr => pr.IsGranted)
                .ToTask();
        }

        
        private async Task<bool> HasAllPermissions(params string[] permissions)
        {
            foreach (var permission in permissions)
            {
                if (!await ClientFunctions.PermissionGranted(permission))
                    return false;
            }
            return true;
        }
        
        private ISharedPreferences GetPreferences() =>
            Application.Context.GetSharedPreferences(SharedPreferenceName, FileCreationMode.Private);

        private void WriteKey(string key, string value)
        {
            using var p = GetPreferences();
            using var editor = p.Edit();
            editor.PutString(key, value);
            editor.Commit();
        }

        private void WriteKey(string key, float value)
        {
            using var p = GetPreferences();
            using var editor = p.Edit();
            editor.PutFloat(key, value);
            editor.Commit();
        }

        private void WriteKey(string key, bool value)
        {
            using var p = GetPreferences();
            using var editor = p.Edit();
            editor.PutBoolean(key, value);
            editor.Commit();
        }

        private void WriteKey(string key, long value)
        {
            using var p = GetPreferences();
            using var editor = p.Edit();
            editor.PutLong(key, value);
            editor.Commit();
        }
        
        private string ReadString(string key, string defaultValue)
        {
            using var p = GetPreferences();
            return p.GetString(key, defaultValue);
        }

        private bool ReadBool(string key, bool defaultValue)
        {
            using var p = GetPreferences();
            return p.GetBoolean(key, defaultValue);
        }
        
        private long ReadLong(string key, long defaultValue)
        {
            using var p = GetPreferences();
            return p.GetLong(key, defaultValue);
        }
        
        private float ReadFloat(string key, float defaultValue)
        {
            using var p = GetPreferences();
            return p.GetFloat(key, defaultValue);
        }
    }
}