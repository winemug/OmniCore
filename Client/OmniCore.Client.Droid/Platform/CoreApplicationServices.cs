using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Services;
using Unity;
using Environment = Android.OS.Environment;

namespace OmniCore.Client.Droid.Platform
{
    public class CoreApplicationServices : ICoreApplicationServices
    {
        private readonly IUnityContainer Container;
        public CoreApplicationServices(ICoreApplicationLogger coreApplicationLogger, IUnityContainer container)
        {
            ApplicationLogger = coreApplicationLogger;
            Container = container;
        }

        public ICoreApplicationLogger ApplicationLogger { get; }
        
        public IUserInterface UserInterface { get; }
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

        public void Shutdown()
        {
            if (Application.Context is Activity activity)
                activity.FinishAffinity();
            else
            {
                Process.KillProcess(Process.MyPid());
            }
        }

        public IBackgroundTask CreateBackgroundTask()
        {
            return Container.Resolve<IBackgroundTask>();
        }

        public IObservable<IUserInterface> WhenStarted()
        {
            throw new NotImplementedException();
        }

        public IObservable<IUserInterface> WhenHibernating()
        {
            throw new NotImplementedException();
        }

        public IObservable<IUserInterface> WhenResuming()
        {
            throw new NotImplementedException();
        }
    }
}