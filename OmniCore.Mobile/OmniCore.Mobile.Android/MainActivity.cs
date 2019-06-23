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
using OmniCore.Mobile.Base.Interfaces;
using System.IO;
using Environment = System.Environment;

namespace OmniCore.Mobile.Android
{
    [Activity(Label = "OmniCore", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(savedInstanceState);
            CrossBleAdapter.AndroidConfiguration.ShouldInvokeOnMainThread = false;
            CrossBleAdapter.AndroidConfiguration.UseInternalSyncQueue = false;
            CrossBleAdapter.AndroidConfiguration.UseNewScanner = true;
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, savedInstanceState);

            Xamarin.Forms.Forms.SetFlags("CollectionView_Experimental");
            Xamarin.Forms.Forms.Init(this, savedInstanceState);

            DependencyService.Register<IRemoteRequestPublisher, RemoteRequestPublisher>();
            DependencyService.Register<IOmniCoreApplication, OmniCoreApplication>();
            DependencyService.Register<IOmniCoreLogger, OmniCoreLogger>();

            var i = new Intent(this, typeof(OmniCoreIntentService));
            i.SetAction(OmniCoreIntentService.ACTION_START_SERVICE);
            StartService(i);

            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

}