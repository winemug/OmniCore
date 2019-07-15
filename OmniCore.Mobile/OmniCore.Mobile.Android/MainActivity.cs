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
using OmniCore.Model.Eros;

namespace OmniCore.Mobile.Android
{
    [Activity(Label = "OmniCore", Icon = "@mipmap/ic_omnicore", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        LaunchMode = LaunchMode.SingleTask, Exported = true, AlwaysRetainTaskState = true,
        Name = "OmniCore.MainActivity")]
    [IntentFilter(new[] { MainActivity.IntentEnsureServiceRunning })]

    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static bool IsCreated { get; private set; }
        public const string IntentEnsureServiceRunning = "EnsureServiceRunning";
        protected override async void OnCreate(Bundle savedInstanceState)
        {
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
            DependencyService.Register<IAppState, OmniCoreAppState>();

            await ErosRepository.GetInstance().ConfigureAwait(true);
            LoadApplication(new App());
            IsCreated = true;
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