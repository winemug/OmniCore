using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client.Droid.Platform
{
    public class ApplicationService : IApplicationService
    {
        public ApplicationService(IApplicationLogger applicationLogger)
        {
            Logger = applicationLogger;
        }

        public IApplicationLogger Logger { get; }
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