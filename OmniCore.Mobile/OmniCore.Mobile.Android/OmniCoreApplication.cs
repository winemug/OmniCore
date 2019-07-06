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
using OmniCore.Model.Exceptions;
using Environment = Android.OS.Environment;

namespace OmniCore.Mobile.Android
{
    [Fody.ConfigureAwait(false)]
    public class OmniCoreApplication : IOmniCoreApplication
    {
        public void Exit()
        {
            if (Application.Context is Activity activity)
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

        public IWakeLock NewBluetoothWakeLock(string name)
        {
            PowerManager pm = (PowerManager)Application.Context.GetSystemService(Context.PowerService);
            var wakeLock = pm.NewWakeLock(WakeLockFlags.Partial | WakeLockFlags.LocationModeNoChange, name);
            return new OmniCoreWakeLock(wakeLock);
        }
    }
}