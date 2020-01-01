using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Android;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using OmniCore.Client.Droid.Services;
using OmniCore.Model.Interfaces.Platform;
using Plugin.BluetoothLE;
using Permission = Android.Content.PM.Permission;
using System.Reactive.Subjects;
using OmniCore.Model.Interfaces;
using OmniCore.Services;
using Application = Xamarin.Forms.Application;
using Debug = System.Diagnostics.Debug;

namespace OmniCore.Client.Droid
{
    [Activity(Label = "OmniCore", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        LaunchMode = LaunchMode.SingleTask, Exported = true, AlwaysRetainTaskState = false,
        Name = "OmniCore.MainActivity")]

    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private bool savedValue = false;

        private ICoreContainer ClientContainer;

        public MainActivity()
        {
            ClientContainer = Initializer.AndroidClientContainer()
                .WithXamarinForms();
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            Debug.WriteLine("ONCREATE");
            savedValue = true;
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            
            base.OnCreate(savedInstanceState);

            CrossBleAdapter.AndroidConfiguration.ShouldInvokeOnMainThread = false;
            CrossBleAdapter.AndroidConfiguration.UseInternalSyncQueue = true;
            CrossBleAdapter.AndroidConfiguration.UseNewScanner = true;

            Xamarin.Forms.Forms.SetFlags("CollectionView_Experimental");
            Xamarin.Forms.Forms.SetFlags("IndicatorView_Experimental");
            Xamarin.Forms.Forms.Init(this, savedInstanceState);

            if (!CheckPermissions().Wait())
            {
                this.FinishAffinity();
            }

            if (!ConnectToService())
            {
                this.FinishAffinity();
            }
            
            LoadApplication((Application) ClientContainer.Get<IPlatformApplication>());
        }

        private bool ConnectToService()
        {
            var servicesConnection = ClientContainer.Get<ICoreServicesConnection>();
            return servicesConnection.Connect();
        }

        private ISubject<bool> PermissionResultSubject;
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            PermissionResultSubject.OnNext(grantResults.All(r => r == Permission.Granted));
            PermissionResultSubject.OnCompleted();
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private IObservable<bool> CheckPermissions()
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
                PermissionResultSubject = new Subject<bool>();
                ActivityCompat.RequestPermissions(this, permissions.ToArray(), 34);
                return PermissionResultSubject.AsObservable();
            }
            return Observable.Return(true);
        }

        protected override void OnStart()
        {
            Debug.WriteLine($"ONSTART {savedValue}");
            savedValue = true;
            base.OnStart();
        }

        protected override void OnResume()
        {
            Debug.WriteLine($"ONRESUME {savedValue}");
            savedValue = true;
            base.OnResume();
        }

        protected override void OnPause()
        {
            Debug.WriteLine($"ONPAUSE {savedValue}");
            savedValue = true;
            base.OnPause();
        }

        protected override void OnStop()
        {
            Debug.WriteLine($"ONSTOP {savedValue}");
            savedValue = true;
            base.OnStop();
        }

        protected override void OnRestart()
        {
            Debug.WriteLine($"ONRESTART {savedValue}");
            savedValue = true;
            base.OnRestart();
        }

        protected override void OnDestroy()
        {
            Debug.WriteLine($"ONDESTROY {savedValue}");
            savedValue = true;
            base.OnDestroy();
        }
    }
}