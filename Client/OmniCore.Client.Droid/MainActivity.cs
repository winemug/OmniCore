using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Java.Util.Logging;
using Microsoft.AppCenter.Push;
using Nito.AsyncEx.Synchronous;
using OmniCore.Client.Droid.Platform;
using OmniCore.Client.Droid.Services;
using OmniCore.Model.Constants;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Utilities.Extensions;
using Rg.Plugins.Popup;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;


namespace OmniCore.Client.Droid
{
    [Activity(Label = "OmniCore", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        LaunchMode = LaunchMode.SingleTask, Exported = true, AlwaysRetainTaskState = false,
        Name = "OmniCore.MainActivity")]
    public class MainActivity : FormsAppCompatActivity, IActivityContext
    {
        private const string WriteExternalStorage = "android.permission.WRITE_EXTERNAL_STORAGE";
        private const string ReadExternalStorage = "android.permission.READ_EXTERNAL_STORAGE";

        private const string Bluetooth = "android.permission.BLUETOOTH";
        private const string BluetoothAdmin = "android.permission.BLUETOOTH_ADMIN";
        private const string BluetoothPrivileged = "android.permission.BLUETOOTH_PRIVILEGED";
        private const string AccessCoarseLocation = "android.permission.ACCESS_COARSE_LOCATION";

        private readonly ConcurrentDictionary<int, ISubject<(string Permission, bool Granted)>>
            PermissionRequestsDictionary =
                new ConcurrentDictionary<int, ISubject<(string Permission, bool Granted)>>();

        private int NextPermissionRequestId = 0;

        private ForegroundTaskServiceConnection ForegroundTaskServiceConnection;
        
        public Task AttachToService(Type concreteType, IClientConnection connection)
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

        public Task DetachFromService(IClientConnection connection)
        {
            var serviceConnection = connection as IServiceConnection;
            if (serviceConnection == null)
            {
                throw new OmniCoreWorkflowException(FailureType.PlatformGeneralError,
                    "Client connection  is not of expected type for the Android platform");
            }
           
            return Device.InvokeOnMainThreadAsync(() => { base.UnbindService(serviceConnection); });
        }

        public IObservable<(string Permission, bool IsGranted)> RequestPermissions(params string[] permissions)
        {
            var requestId = Interlocked.Increment(ref NextPermissionRequestId);
            var permissionSubject = new ReplaySubject<(string Permission, bool Granted)>();
            PermissionRequestsDictionary[requestId] = permissionSubject;
            Device.InvokeOnMainThreadAsync(() =>
            {
                ActivityCompat.RequestPermissions(this, permissions.ToArray(), requestId);
            });
            return permissionSubject.AsObservable();
        }

        public async Task<bool> PermissionGranted(string permission)
        {
            return await Device.InvokeOnMainThreadAsync(() => ContextCompat.CheckSelfPermission(this, permission) ==
                                                              (int) Permission.Granted);
        }

        public void Exit()
        {
            FinishAffinity();
        }

        public async Task<bool> BluetoothPermissionGranted()
        {
            return await HasAllPermissions(Bluetooth,
                BluetoothAdmin, BluetoothPrivileged, AccessCoarseLocation);
        }

        public async Task<bool> StoragePermissionGranted()
        {
            return await HasAllPermissions(ReadExternalStorage,
                WriteExternalStorage);
        }

        public async Task<bool> RequestBluetoothPermission()
        {
            return await RequestPermissions(Bluetooth,
                    BluetoothAdmin, BluetoothPrivileged, AccessCoarseLocation)
                .All(pr => pr.IsGranted)
                .ToTask();
        }

        public async Task<bool> RequestStoragePermission()
        {
            return await RequestPermissions(ReadExternalStorage,
                    WriteExternalStorage)
                .All(pr => pr.IsGranted)
                .ToTask();
        }

        
        private async Task<bool> HasAllPermissions(params string[] permissions)
        {
            foreach (var permission in permissions)
            {
                if (!await PermissionGranted(permission))
                    return false;
            }
            return true;
        }

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            AndroidContainer.Instance.Existing<IActivityContext>(this);
            
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
           
            Forms.SetFlags("CollectionView_Experimental",
                "IndicatorView_Experimental", "CarouselView_Experimental");

            Forms.Init(this, savedInstanceState);
            Popup.Init(this, savedInstanceState);

            // XdripReceiver = new GenericBroadcastReceiver();
            // RegisterReceiver(XdripReceiver, new IntentFilter("com.eveningoutpost.dexdrip.BgEstimate"));
            
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
            AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironmentOnUnhandledExceptionRaiser;

            ForegroundTaskServiceConnection = new ForegroundTaskServiceConnection();
            
            base.OnCreate(savedInstanceState);

            var client = await AndroidContainer.Instance.Get<IClient>();
            LoadApplication(client as Xamarin.Forms.Application);
        }
        private async void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var sb = new StringBuilder()
                .AppendLine("Caught unhandled exception in application domain")
                .AppendLine($"Is Terminating: {e.IsTerminating}");

            if (e.ExceptionObject == null)
            {
                sb.AppendLine("Exception object is NULL");
            }
            else
            {
                var eo = e.ExceptionObject as Exception;
                if (eo == null)
                {
                    sb.AppendLine($"Exception object is of type {e.ExceptionObject.GetType()}");
                    sb.AppendLine($"ToString(): {e.ExceptionObject.ToString()}");
                }
                else
                {
                    sb.AppendLine($"Exception: {eo.AsDebugFriendly()}");
                }
            }
            await LogError(sb.ToString());
        }

        private async void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var sb = new StringBuilder()
                .AppendLine("Task scheduled caught an unhandled exception!")
                .AppendLine($"Observed: {e.Observed}");
            if (e.Exception == null)
                sb.AppendLine("Exception: NULL");
            else
                sb.AppendLine($"Exception: {e.Exception.AsDebugFriendly()}");
            await LogError(sb.ToString());
        }

        private async void AndroidEnvironmentOnUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            var sb = new StringBuilder()
                .AppendLine("Android environment caught an unhandled exception!")
                .AppendLine($"Is Handled: {e.Handled}");
            if (!e.Handled)
            {
                sb.AppendLine("Setting handled to true.");
                e.Handled = true;
            }
            if (e.Exception == null)
                sb.AppendLine("Exception: NULL");
            else
                sb.AppendLine($"Exception: {e.Exception.AsDebugFriendly()}");
            await LogError(sb.ToString());
        }

        private async Task LogError(string logText)
        {
            var logger = await AndroidContainer.Instance.Get<ILogger>();
            logger.Error(LoggingConstants.Tag, logText);
        }

        public override void OnBackPressed()
        {
            if (Popup.SendBackPressed(base.OnBackPressed))
            {
                // Do something if there are some pages in the `PopupStack`
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            Push.CheckLaunchedFromNotification(this, intent);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            [GeneratedEnum] Permission[] grantResults)
        {
            if (PermissionRequestsDictionary.TryGetValue(requestCode, out var subject))
            {
                var count = Math.Min(permissions.Length, grantResults.Length);
                for (int i = 0; i < count; i++)
                {
                    subject.OnNext((permissions[i], grantResults[i] == Permission.Granted));
                }
                
                subject.OnCompleted();
            }
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public Task<IForegroundTaskService> GetForegroundTaskService(CancellationToken cancellationToken)
        {
            if (!ForegroundTaskServiceConnection.IsConnected)
            {
                Device.InvokeOnMainThreadAsync(() =>
                {
                    var intent = new Intent(this, typeof(ForegroundTaskService));
                    if (!BindService(intent, ForegroundTaskServiceConnection, Bind.AutoCreate))
                        throw new Exception("Failed to connect to local service");
                });
            }
            return ForegroundTaskServiceConnection.WhenConnected().ToTask(cancellationToken);
        }

        protected override async void OnPause()
        {
            await Device.InvokeOnMainThreadAsync(() =>
            {
                if (ForegroundTaskServiceConnection.IsConnected)
                {
                    var intent = new Intent(this, typeof(ForegroundTaskService));
                    UnbindService(ForegroundTaskServiceConnection);
                }
            });
        }
    }
}