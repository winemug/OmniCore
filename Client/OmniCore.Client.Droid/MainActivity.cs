using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
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
    public class MainActivity : FormsAppCompatActivity, IUserActivity
    {
        private const string WriteExternalStorage = "android.permission.WRITE_EXTERNAL_STORAGE";
        private const string ReadExternalStorage = "android.permission.READ_EXTERNAL_STORAGE";

        private const string Bluetooth = "android.permission.BLUETOOTH";
        private const string AccessCoarseLocation = "android.permission.ACCESS_COARSE_LOCATION";

        private readonly ConcurrentDictionary<int, ISubject<(string Permission, bool Granted)>>
            PermissionRequestsDictionary =
                new ConcurrentDictionary<int, ISubject<(string Permission, bool Granted)>>();

        private int NextPermissionRequestId = 0;
        private ForegroundTaskServiceConnection ForegroundTaskServiceConnection;
        
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
        public async Task<bool> BluetoothPermissionGranted()
        {
            return await HasAllPermissions(Bluetooth, AccessCoarseLocation);
        }

        public async Task<bool> StoragePermissionGranted()
        {
            return await HasAllPermissions(ReadExternalStorage,
                WriteExternalStorage);
        }

        public async Task<bool> RequestBluetoothPermission()
        {
            return await RequestPermissions(Bluetooth, AccessCoarseLocation)
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
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
           
            Forms.SetFlags("CollectionView_Experimental",
                "IndicatorView_Experimental", "CarouselView_Experimental");

            Forms.Init(this, savedInstanceState);
            Popup.Init(this, savedInstanceState);

            base.OnCreate(savedInstanceState);

            // XdripReceiver = new GenericBroadcastReceiver();
            // RegisterReceiver(XdripReceiver, new IntentFilter("com.eveningoutpost.dexdrip.BgEstimate"));
            
            AndroidContainer.Instance.Existing<IUserActivity>(this);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
            AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironmentOnUnhandledExceptionRaiser;

            ForegroundTaskServiceConnection = new ForegroundTaskServiceConnection();
            
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
            base.OnBackPressed();
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

        public async Task StartForegroundTaskService(CancellationToken cancellationToken)
        {
            if (!ForegroundTaskServiceConnection.IsConnected)
            {
                await Device.InvokeOnMainThreadAsync(() =>
                {
                    var intent = new Intent(this, typeof(ForegroundTaskService));
                    if (!BindService(intent, ForegroundTaskServiceConnection, Bind.AutoCreate))
                        throw new Exception("Failed to connect to local service");
                });
            }
            await ForegroundTaskServiceConnection.WhenConnected().ToTask(cancellationToken);
        }

        public async Task StopForegroundTaskService(CancellationToken cancellationToken)
        {
            if (ForegroundTaskServiceConnection.IsConnected)
            {
                await Device.InvokeOnMainThreadAsync(() =>
                {
                    UnbindService(ForegroundTaskServiceConnection);
                });
            }
        }

        // protected override async void OnPause()
        // {
        //     await Device.InvokeOnMainThreadAsync(() =>
        //     {
        //         if (ForegroundTaskServiceConnection.IsConnected)
        //         {
        //             var intent = new Intent(this, typeof(ForegroundTaskService));
        //             UnbindService(ForegroundTaskServiceConnection);
        //         }
        //     });
        // }
    }
}