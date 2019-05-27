using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.Permissions;
using Android.Content;
using Plugin.BluetoothLE;
using System.Runtime.InteropServices;
using Mono.Data.Sqlite;

namespace OmniCore.Mobile.Droid
{
    [Activity(Label = "OmniCore.Mobile", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        [DllImport("libsqlite.so")]
        internal static extern int sqlite3_shutdown();

        [DllImport("libsqlite.so")]
        internal static extern int sqlite3_initialize();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            sqlite3_shutdown();
            SqliteConnection.SetConfig(SQLiteConfig.Serialized);
            sqlite3_initialize();

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, savedInstanceState);

            CrossBleAdapter.AndroidConfiguration.ShouldInvokeOnMainThread = false;
            CrossBleAdapter.AndroidConfiguration.UseInternalSyncQueue = false;
            CrossBleAdapter.AndroidConfiguration.UseNewScanner = true;

            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

    }

}