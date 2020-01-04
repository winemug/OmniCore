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
using System.Threading.Tasks;
using Android.Content;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Extensions;
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
        private ICoreClientConnection CoreClientConnection;
        private IServiceConnection ServiceConnection => CoreClientConnection as IServiceConnection;
        private bool ConnectRequested = false;
        private bool DisconnectRequested = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            AppDomain.CurrentDomain.UnhandledException += (sender, args) => OnUnhandledException(args.ExceptionObject);
            TaskScheduler.UnobservedTaskException += (sender, args) => OnUnhandledException(args.Exception);
            AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) => OnUnhandledException(args.Exception);

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

            ClientContainer = Initializer.AndroidClientContainer(this)
                .WithXamarinForms();

            CoreClientConnection = ClientContainer.Get<ICoreClientConnection>();

            LoadXamarinApplication();

        }

        private void OnUnhandledException(object exceptionObject)
        {
            if (exceptionObject != null && exceptionObject is Exception e)
            {
                Debug.WriteLine(e.AsDebugFriendly());
            }
            else
            {
                Debug.WriteLine($"****** Unknown exception object {exceptionObject}");
            }
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

        protected override void OnResume()
        {
            ConnectToAndroidService();
            base.OnResume();
        }

        protected override void OnStop()
        {
            base.OnStop();
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
            DisconnectRequested = false;
        }

        private void DisconnectFromAndroidService()
        {
            if (DisconnectRequested)
                return;
            
            base.UnbindService(ServiceConnection);
            ConnectRequested = false;
            DisconnectRequested = true;
        }
    }
}