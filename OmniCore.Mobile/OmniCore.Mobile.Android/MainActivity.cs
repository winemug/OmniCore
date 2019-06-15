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
using OmniCore.Mobile.Interfaces;
using System.IO;
using Environment = System.Environment;

namespace OmniCore.Mobile.Android
{
    [Activity(Label = "OmniCore.Mobile", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            var logDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "logs");
            var diLogs = new DirectoryInfo(logDirPath);
            if (!diLogs.Exists)
            {
                diLogs.Create();
            }

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, savedInstanceState);

            //CrossBleAdapter.AndroidConfiguration.ShouldInvokeOnMainThread = false;
            //CrossBleAdapter.AndroidConfiguration.UseInternalSyncQueue = false;
            //CrossBleAdapter.AndroidConfiguration.UseNewScanner = true;

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