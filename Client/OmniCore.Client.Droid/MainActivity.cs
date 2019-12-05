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
using System.IO;
using OmniCore.Eros;
using OmniCore.Services;
using Unity;
using OmniCore.Mobile.Droid;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Radios.RileyLink;
using OmniCore.Repository.Sqlite;
using OmniCore.Simulation;
using Application = Xamarin.Forms.Application;

namespace OmniCore.Client.Droid
{
    [Activity(Label = "OmniCore", Icon = "@mipmap/ic_omnicore", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        LaunchMode = LaunchMode.SingleTask, Exported = true, AlwaysRetainTaskState = false,
        Name = "OmniCore.MainActivity")]

    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static bool IsCreated { get; private set; }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            var container = new UnityContainer()
                .WithDefaultServiceProviders()
                .WithSqliteRepository()
#if EMULATOR
                .WithOmnipodEros()
                .WithRileyLinkRadio()
                .WithBleSimulator()
#else
                .WithOmnipodEros()
                .WithRileyLinkRadio()
                .WithCrossBleAdapter()
#endif
                .AsXamarinApplication()
                .OnAndroidPlatform();
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(savedInstanceState);

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, savedInstanceState);
            CrossBleAdapter.AndroidConfiguration.ShouldInvokeOnMainThread = false;
            CrossBleAdapter.AndroidConfiguration.UseInternalSyncQueue = false;
            CrossBleAdapter.AndroidConfiguration.UseNewScanner = true;

            Xamarin.Forms.Forms.SetFlags("CollectionView_Experimental");
            Xamarin.Forms.Forms.Init(this, savedInstanceState);

            var uiApplication = container.Resolve<IUserInterface>();

            LoadApplication(uiApplication as Application);
            IsCreated = true;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}