using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common.Apis;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Microsoft.AppCenter.Push;
using OmniCore.Client.Droid.Services;
using OmniCore.Mobile.Droid.Platform;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Common;
using Rg.Plugins.Popup;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace OmniCore.Client.Droid
{
    [Activity(Label = "OmniCore", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        LaunchMode = LaunchMode.SingleTask, Exported = true, AlwaysRetainTaskState = false,
        Name = "OmniCore.MainActivity")]
    public class MainActivity : FormsAppCompatActivity, ICorePlatformClient
    {
        private ICoreContainer<IClientResolvable> ClientContainer;
        private bool ConnectRequested;
        private bool DisconnectRequested;

        private ISubject<bool> PermissionResultSubject;
        
        // private GenericBroadcastReceiver XdripReceiver;

        private IServiceConnection ServiceConnection =>
            (IServiceConnection) ClientContainer.Get<ICoreClientConnection>();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            SynchronizationContext = SynchronizationContext.Current;
            
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            AppDomain.CurrentDomain.UnhandledException += (sender, args) => OnUnhandledException(args.ExceptionObject);
            TaskScheduler.UnobservedTaskException += (sender, args) => OnUnhandledException(args.Exception);
            AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) => OnUnhandledException(args.Exception);

            Forms.SetFlags("CollectionView_Experimental",
                "IndicatorView_Experimental", "CarouselView_Experimental");

            base.OnCreate(savedInstanceState);

            Forms.Init(this, savedInstanceState);

            //TODO: move to service
            if (!CheckPermissions().Wait()) FinishAffinity();

            // XdripReceiver = new GenericBroadcastReceiver();
            // RegisterReceiver(XdripReceiver, new IntentFilter("com.eveningoutpost.dexdrip.BgEstimate"));

            Popup.Init(this, savedInstanceState);

            ClientContainer = Initializer.AndroidClientContainer(this)
                .WithXamarinFormsClient();

            LoadApplication(ClientContainer.Get<ICoreClient>() as Xamarin.Forms.Application);
        }

        public override void OnBackPressed()
        {
            if (Popup.SendBackPressed(base.OnBackPressed))
            {
                // Do something if there are some pages in the `PopupStack`
            }
        }

        private void OnUnhandledException(object exceptionObject)
        {
            if (exceptionObject != null && exceptionObject is Exception e)
                CoreLoggingFunctions.FatalError("Unhandled Exception", e);
            else
                CoreLoggingFunctions.FatalError($"Unhandled Exception - Unknown exception object {exceptionObject}");
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            [GeneratedEnum] Permission[] grantResults)
        {
            PermissionResultSubject.OnNext(grantResults.All(r => r == Permission.Granted));
            PermissionResultSubject.OnCompleted();
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private IObservable<bool> CheckPermissions()
        {
            var permissions = new List<string>
            {
                Manifest.Permission.AccessCoarseLocation,
                Manifest.Permission.BluetoothPrivileged,
                Manifest.Permission.ReadExternalStorage,
                Manifest.Permission.WriteExternalStorage
            };

            foreach (var permission in permissions.ToArray())
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) ==
                    (int) Permission.Granted)
                    permissions.Remove(permission);

            if (permissions.Count > 0)
            {
                PermissionResultSubject = new Subject<bool>();
                ActivityCompat.RequestPermissions(this, permissions.ToArray(), 34);
                return PermissionResultSubject.AsObservable();
            }
            return Observable.Return(true);
        }
        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            Push.CheckLaunchedFromNotification(this, intent);
        }

        public Task AttachToService(Type concreteType, ICoreClientConnection connection)
        {
            var serviceConnection = connection as IServiceConnection;
            if (serviceConnection == null)
            {
                throw new OmniCoreWorkflowException(FailureType.PlatformGeneralError,
                    "Client connection  is not of expected type for the Android platform");
            }
            return Device.InvokeOnMainThreadAsync(() =>
            {
                var intent = new Intent(this, concreteType);
                if (!BindService(intent, connection as IServiceConnection, Bind.AutoCreate))
                    throw new OmniCoreUserInterfaceException(FailureType.ServiceConnectionFailed);
            });
        }

        public Task DetachFromService(ICoreClientConnection connection)
        {
            var serviceConnection = connection as IServiceConnection;
            if (serviceConnection == null)
            {
                throw new OmniCoreWorkflowException(FailureType.PlatformGeneralError,
                    "Client connection  is not of expected type for the Android platform");
            }
           
            return Device.InvokeOnMainThreadAsync(() => { base.UnbindService(serviceConnection); });
        }

        public SynchronizationContext SynchronizationContext { get; private set; }
    }
}