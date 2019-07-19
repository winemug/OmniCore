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
using OmniCore.Mobile.Base;

namespace OmniCore.Mobile.Android
{
    [Activity(Label = "OmniCore", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        LaunchMode = LaunchMode.SingleTask, Exported = true,
        Name = "OmniCore.MainActivity")]
    [IntentFilter(new[] { MainActivity.IntentEnsureServiceRunning })]

    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static bool IsCreated { get; private set; }
        public const string IntentEnsureServiceRunning = "EnsureServiceRunning";
        protected override void OnCreate(Bundle savedInstanceState)
        {
            ToggleAdapter();

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(savedInstanceState);
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, savedInstanceState);
            CrossBleAdapter.AndroidConfiguration.ShouldInvokeOnMainThread = false;
            CrossBleAdapter.AndroidConfiguration.UseInternalSyncQueue = false;
            CrossBleAdapter.AndroidConfiguration.UseNewScanner = true;

            Xamarin.Forms.Forms.SetFlags("CollectionView_Experimental");
            Xamarin.Forms.Forms.Init(this, savedInstanceState);

            DependencyService.Register<IRemoteRequestPublisher, RemoteRequestPublisher>();
            DependencyService.Register<IOmniCoreApplication, OmniCoreApplication>();
            DependencyService.Register<IOmniCoreLogger, OmniCoreLogger>();

            LoadApplication(new App());
            var i = new Intent(this, typeof(OmniCoreIntentService));
            i.SetAction(OmniCoreIntentService.ACTION_START_SERVICE);
            StartService(i);
            IsCreated = true;
        }

        private void ToggleAdapter()
        {
            try
            {
                if (CrossBleAdapter.Current.CanControlAdapterState())
                {
                    // Switch adapter on if off (testing)
                    if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOff)
                    {
                        CrossBleAdapter.Current.SetAdapterState(true);
                        Wait(5000);
                    }
                }
            }
            catch
            {
                // TODO: We'll handle this later...
                // Try continuing
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnNewIntent(Intent intent)
        {
            OmniCoreServices.Logger.Debug("MainActivity: new intent received");
            if (intent.Action == IntentEnsureServiceRunning)
            {
                OmniCoreServices.Logger.Debug("MainActivity: EnsureServiceRunning");
            }
        }
    }
}