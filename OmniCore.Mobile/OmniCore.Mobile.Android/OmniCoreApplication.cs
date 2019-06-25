using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OmniCore.Mobile.Base.Interfaces;
using Environment = Android.OS.Environment;

namespace OmniCore.Mobile.Android
{
    [Fody.ConfigureAwait(false)]
    public class OmniCoreApplication : IOmniCoreApplication
    {
        public void Exit()
        {
            var activity = Application.Context as Activity;
            if (activity != null)
                activity.FinishAffinity();
            else
            {
                Process.KillProcess(Process.MyPid());
            }
        }

        public string Version
        {
            get
            {
                Context context = global::Android.App.Application.Context;
                PackageManager manager = context.PackageManager;
                PackageInfo info = manager.GetPackageInfo(context.PackageName, 0);
                return info.VersionName;
            }
        }

        public async Task RunOnMainThread(Func<Task> asyncFunction)
        {
            await Xamarin.Forms.Device.InvokeOnMainThreadAsync(asyncFunction);
        }

        public async Task<T> RunOnMainThread<T>(Func<Task<T>> asyncFunction)
        {
            return await Xamarin.Forms.Device.InvokeOnMainThreadAsync(asyncFunction);
        }

        public async Task RunOnMainThread(Action action)
        {
            await Xamarin.Forms.Device.InvokeOnMainThreadAsync(action);
        }

        public async Task<T> RunOnMainThread<T>(Func<T> function)
        {
            return await Xamarin.Forms.Device.InvokeOnMainThreadAsync(function);
        }

        public async Task<SynchronizationContext> GetMainSyncContext()
        {
            return await Xamarin.Forms.Device.GetMainThreadSynchronizationContextAsync();
        }

        public string GetPublicDataPath()
        {
            var storagePath = Environment.ExternalStorageDirectory.Path;
            return Path.Combine(storagePath, "omnicore");
        }

        private PowerManager.WakeLock wakeLock;

        public void AcquireWakeLock()
        {
            if (this.wakeLock != null)
            {
                PowerManager pm = (PowerManager)Application.Context.GetSystemService(Context.PowerService);
                this.wakeLock = pm.NewWakeLock(WakeLockFlags.Partial | WakeLockFlags.LocationModeNoChange, "OmniCorePodOperation");
                this.wakeLock.Acquire();
            }
        }

        public void ReleaseWakeLock()
        {
            if (this.wakeLock != null)
            {
                this.wakeLock.Release();
                this.wakeLock = null;
            }
        }

        //public async Task RunOnUi(Func<Task> action)
        //{
        //    Xamarin.Forms.Device.InvokeOnMainThreadAsync()
        //}

        //public void RunOnUiThread(Action action)
        //{
        //    Xamarin.Forms.Device.BeginInvokeOnMainThread(action);
        //}

        //public async Task RunOnUiThreadAndSync(Action<TaskCompletionSource<bool>> syncedAction)
        //{
        //    var tcs = new TaskCompletionSource<bool>();
        //    Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
        //    {
        //       try
        //       {
        //           syncedAction(tcs);
        //       }
        //       catch(Exception e)
        //       {
        //           tcs.TrySetException(e);
        //       }
        //    });
        //    await tcs.Task;
        //}

        //public async Task<T> RunOnUiThread<T>(Func<T> function)
        //{
        //   Xamarin.Forms.Device.BeginInvokeOnMainThread
        //   return await Xamarin.Forms.Device.InvokeOnMainThreadAsync<T>( () =>
        //   {
        //       return function.Invoke();
        //   });
        //}

        //public async Task RunOnUiThread(Action action)
        //{
        //    await Xamarin.Forms.Device.InvokeOnMainThreadAsync(() =>
        //    {
        //        action.Invoke();
        //    });
        //}
    }
}