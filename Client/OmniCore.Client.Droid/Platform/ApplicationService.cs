using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Services;
using Environment = Android.OS.Environment;

namespace OmniCore.Client.Droid.Platform
{
    public class ApplicationService : IApplicationService
    {
        public ApplicationService(IApplicationLogger applicationLogger)
        {
            Logger = applicationLogger;
        }

        public IApplicationLogger Logger { get; }
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
    }
}