using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Microsoft.AppCenter.Push;
using OmniCore.Client.Droid.Services;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Utilities.Extensions;
using Rg.Plugins.Popup;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Debug = System.Diagnostics.Debug;

namespace OmniCore.Client.Droid
{
    [Activity(Label = "OmniCore", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        LaunchMode = LaunchMode.SingleTask, Exported = true, AlwaysRetainTaskState = false,
        Name = "OmniCore.MainActivity")]
    public class MainActivity : FormsAppCompatActivity, ICoreClientContext
    {
        private ICoreContainer<IClientResolvable> ClientContainer;
        private bool ConnectRequested;
        private bool DisconnectRequested;

        private ISubject<bool> PermissionResultSubject;

#if DEBUG
        private IDisposable ScreenLockDisposable;
#endif

        private GenericBroadcastReceiver XdripReceiver;

        private IServiceConnection ServiceConnection =>
            (IServiceConnection) ClientContainer.Get<ICoreClientConnection>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
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

            XdripReceiver = new GenericBroadcastReceiver();
            RegisterReceiver(XdripReceiver, new IntentFilter("com.eveningoutpost.dexdrip.BgEstimate"));


            Popup.Init(this, savedInstanceState);

            ClientContainer = Initializer.AndroidClientContainer(this)
                .WithXamarinForms();

            LoadXamarinApplication();
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
                Debug.WriteLine(e.AsDebugFriendly());
            else
                Debug.WriteLine($"****** Unknown exception object {exceptionObject}");
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

        protected override void OnResume()
        {
#if DEBUG
            ScreenLockDisposable = ClientContainer.Get<ICoreClient>().DisplayKeepAwake();
#endif
            ConnectToAndroidService();
            base.OnResume();
        }

        protected override void OnPause()
        {
#if DEBUG
            ScreenLockDisposable?.Dispose();
            ScreenLockDisposable = null;
#endif
            base.OnPause();
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

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            Push.CheckLaunchedFromNotification(this, intent);
        }
    }
}