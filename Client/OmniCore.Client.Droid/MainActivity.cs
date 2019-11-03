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
using System.Security;
using Xamarin.Forms;
using OmniCore.Client;
using OmniCore.Client.Interfaces;
using System.IO;
using Unity;
using OmniCore.Mobile.Droid;

namespace OmniCore.Client.Droid
{
    [Activity(Label = "OmniCore", Icon = "@mipmap/ic_omnicore", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        LaunchMode = LaunchMode.SingleTask, Exported = true, AlwaysRetainTaskState = false,
        Name = "OmniCore.MainActivity")]

    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static bool IsCreated { get; private set; }
        public const string IntentEnsureServiceRunning = "EnsureServiceRunning";
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(savedInstanceState);
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, savedInstanceState);
            CrossBleAdapter.AndroidConfiguration.ShouldInvokeOnMainThread = false;
            CrossBleAdapter.AndroidConfiguration.UseInternalSyncQueue = false;
            CrossBleAdapter.AndroidConfiguration.UseNewScanner = true;

            var container = new UnityContainer();
            Initializer.RegisterTypes(container);

            Xamarin.Forms.Forms.SetFlags("CollectionView_Experimental");
            Xamarin.Forms.Forms.Init(this, savedInstanceState);

            LoadApplication(new App(container));
            IsCreated = true;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}