using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Services;
using Unity;
using Environment = Android.OS.Environment;

namespace OmniCore.Client.Droid.Platform
{
    public class CoreApplicationService : OmniCoreService, ICoreApplicationService
    {
        public string DataPath => System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

        public string StoragePath
        {
            get
            {
                var storagePath = Path.Combine(Environment.ExternalStorageDirectory.AbsolutePath, "omnicore");
                if (!Directory.Exists(storagePath))
                {
                    Directory.CreateDirectory(storagePath);
                }

                return storagePath;
            }
        }

        public Version Version
        {
            get
            {
                Context context = global::Android.App.Application.Context;
                PackageManager manager = context.PackageManager;
                PackageInfo info = manager.GetPackageInfo(context.PackageName, 0);
                return new Version(info.VersionName);
            }
        }

        public SynchronizationContext UiSynchronizationContext => Android.App.Application.SynchronizationContext;

        public IDisposable KeepAwake()
        {
            return new KeepAwakeLock();
        }

        IObservable<ICoreApplicationService> ICoreApplicationService.WhenStarted()
        {
            throw new NotImplementedException();
        }

        IObservable<ICoreApplicationService> ICoreApplicationService.WhenHibernating()
        {
            throw new NotImplementedException();
        }

        IObservable<ICoreApplicationService> ICoreApplicationService.WhenResuming()
        {
            throw new NotImplementedException();
        }

        protected override Task OnStart(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override Task OnStop(CancellationToken cancellationToken)
        {
            if (Application.Context is Activity activity)
                activity.FinishAffinity();
            else
            {
                Process.KillProcess(Process.MyPid());
            }

            return Task.CompletedTask;
        }

        protected override Task OnPause(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task OnResume(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}