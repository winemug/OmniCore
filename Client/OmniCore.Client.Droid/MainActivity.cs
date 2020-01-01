using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Nito.AsyncEx;
using OmniCore.Client.Droid.Services;
using Unity;
using OmniCore.Model.Interfaces.Services;
using Plugin.BluetoothLE;
using Application = Xamarin.Forms.Application;
using Permission = Android.Content.PM.Permission;

namespace OmniCore.Client.Droid
{
    [Activity(Label = "OmniCore", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        LaunchMode = LaunchMode.SingleTask, Exported = true, AlwaysRetainTaskState = false,
        Name = "OmniCore.MainActivity")]

    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private TaskCompletionSource<bool> PermissionsRequestResult;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            
            base.OnCreate(savedInstanceState);

            CrossBleAdapter.AndroidConfiguration.ShouldInvokeOnMainThread = false;
            CrossBleAdapter.AndroidConfiguration.UseInternalSyncQueue = true;
            CrossBleAdapter.AndroidConfiguration.UseNewScanner = true;

            Xamarin.Forms.Forms.SetFlags("CollectionView_Experimental");
            Xamarin.Forms.Forms.SetFlags("IndicatorView_Experimental");
            Xamarin.Forms.Forms.Init(this, savedInstanceState);

            PermissionsRequestResult = new TaskCompletionSource<bool>();
            if (ShouldWaitForPermissionsResult())
            {
                if (!await PermissionsRequestResult.Task)
                {
                    Shutdown();
                }
            }

            var startIntent = new Intent(this, typeof(CoreBootstrapper));
            var connection = new CoreServiceConnection();

            if (!BindService(startIntent, connection, Bind.AutoCreate))
            {
                //TODO:
            }

            var bootstrapper = await connection.WhenConnected();
            LoadApplication(new XamarinApp(bootstrapper));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            PermissionsRequestResult.TrySetResult(grantResults.All(r => r == Permission.Granted));
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private bool ShouldWaitForPermissionsResult()
        {
            var permissions = new List<string>()
            {
                Manifest.Permission.AccessCoarseLocation,
                Manifest.Permission.BluetoothAdmin,
                Manifest.Permission.ReadExternalStorage,
                Manifest.Permission.WriteExternalStorage,
            };

            foreach (var permission in permissions.ToArray())
            {
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) ==
                    (int) Permission.Granted)
                    permissions.Remove(permission);
            }

            if (permissions.Count > 0)
            {
                ActivityCompat.RequestPermissions(this, permissions.ToArray(), 34);
                return true;
            }

            return false;
        }

        private void Shutdown()
        {
            this.FinishAffinity();
        }
    }
}