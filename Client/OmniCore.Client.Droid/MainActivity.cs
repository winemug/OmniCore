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
using OmniCore.Client.Droid;
using Plugin.BluetoothLE;
using Permission = Android.Content.PM.Permission;
using System.Reactive.Subjects;
using Android.Content;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
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

    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, ICoreClientContext
    {
        private ICoreContainer<IClientResolvable> ClientContainer;
        private IDisposable ServiceConnectSubscription;
        private IDisposable ServiceDisconnectSubscription;
        private ICoreClientConnection CoreClientConnection;
        private IServiceConnection ServiceConnection => CoreClientConnection as IServiceConnection;
        private bool ConnectRequested = false;
        private bool SavedValue = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            //TODO: move
            CrossBleAdapter.AndroidConfiguration.ShouldInvokeOnMainThread = false;
            CrossBleAdapter.AndroidConfiguration.UseInternalSyncQueue = true;
            CrossBleAdapter.AndroidConfiguration.UseNewScanner = true;

            Xamarin.Forms.Forms.SetFlags("CollectionView_Experimental");
            Xamarin.Forms.Forms.SetFlags("IndicatorView_Experimental");
            Xamarin.Forms.Forms.Init(this, savedInstanceState);

            //TODO: move to service
            if (!CheckPermissions().Wait())
            {
                this.FinishAffinity();
            }

            SavedValue = true;

            ClientContainer = Initializer.AndroidClientContainer(this)
                .WithXamarinForms();

            CoreClientConnection = ClientContainer.Get<ICoreClientConnection>();
            
            ConnectToAndroidService();

            LoadXamarinApplication();

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
            base.OnStart();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override void OnStop()
        {
            base.OnStop();
            DisconnectFromAndroidService();
        }

        protected override void OnRestart()
        {
            base.OnRestart();
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            DisconnectFromAndroidService();
        }

        private void LoadXamarinApplication()
        {
            LoadApplication(ClientContainer.Get<XamarinApp>());
        }
        
        private void ConnectToAndroidService()
        {
            if (ConnectRequested)
                return;
            
            var intent = new Intent(this, typeof(AndroidService));
            if (!BindService(intent, ServiceConnection, Bind.AutoCreate))
                throw new OmniCoreUserInterfaceException(FailureType.ServiceConnectionFailed);
            ConnectRequested = true;
        }

        private void DisconnectFromAndroidService()
        {
            if (!ConnectRequested)
                return;
            
            base.UnbindService(ServiceConnection);
            ConnectRequested = false;
        }

    }
}