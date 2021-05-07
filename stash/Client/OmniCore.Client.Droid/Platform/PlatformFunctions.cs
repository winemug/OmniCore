using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Services;
using Environment = System.Environment;

namespace OmniCore.Client.Droid
{
    public class PlatformFunctions : IPlatformFunctions
    {
        public string DataPath => Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public string StoragePath
        {
            get
            {
                var storagePath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath,
                    "omnicore");
                if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);

                return storagePath;
            }
        }

        public Version Version
        {
            get
            {
                var context = Application.Context;
                var manager = context.PackageManager;
                var info = manager.GetPackageInfo(context.PackageName, 0);
                return new Version(info.VersionName);
            }
        }

        public IDisposable BluetoothLock()
        {
            return new BluetoothLock();
        }

        public void Exit()
        {
            if (Application.Context is Activity activity)
                activity.FinishAffinity();
            else
                Process.KillProcess(Process.MyPid());
        }
    }
}