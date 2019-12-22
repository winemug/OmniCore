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
using OmniCore.Client.Droid.Services;
using OmniCore.Data;
using OmniCore.Eros;
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
        LaunchMode = LaunchMode.SingleTask, Exported = false, AlwaysRetainTaskState = false,
        Name = "OmniCore.MainActivity")]

    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            var container = Initializer.SetupDependencies();
            
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            
            base.OnCreate(savedInstanceState);

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, savedInstanceState);

            Xamarin.Forms.Forms.SetFlags("CollectionView_Experimental");
            Xamarin.Forms.Forms.Init(this, savedInstanceState);

            var serviceConnection = new DroidCoreServiceConnection(); 
            var serviceToStart = new Intent(this, typeof(DroidCoreService));
            if (!BindService(serviceToStart, serviceConnection, Bind.AutoCreate))
            {
                //TODO:
            }
            
            var uiApplication = container.Resolve<IUserInterface>();
            LoadApplication(uiApplication as Application);
        }
        
        

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}