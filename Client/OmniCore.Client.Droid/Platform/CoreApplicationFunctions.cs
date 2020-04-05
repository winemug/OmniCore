using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Services;
using Environment = System.Environment;

namespace OmniCore.Client.Droid
{
    public class CoreApplicationFunctions : CoreServiceBase, ICoreApplicationFunctions
    {
        public SynchronizationContext UiSynchronizationContext => Application.SynchronizationContext;
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

        public IDisposable BluetoothKeepAwake()
        {
            return new BluetoothWakeLock();
        }

        public void StorePreferences((string Key, string Value)[] preferences)
        {
            using var sharedPreferences =
                Application.Context.GetSharedPreferences("OmniCore", FileCreationMode.Private);
            using var editor = sharedPreferences.Edit();
            foreach (var preference in preferences)
                editor.PutString(preference.Key, preference.Value);
            editor.Commit();
        }

        public (string Key, string Value)[] ReadPreferences((string Key, string DefaultValue)[] preferences)
        {
            using var sharedPreferences =
                Application.Context.GetSharedPreferences("OmniCore", FileCreationMode.Private);
            var preferencesResult = new ValueTuple<string, string>[preferences.Length];

            for (var i = 0; i < preferences.Length; i++)
                preferencesResult[i] = (preferences[i].Key,
                    sharedPreferences.GetString(preferences[i].Key, preferences[i].DefaultValue));

            return preferencesResult;
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
                Process.KillProcess(Process.MyPid());

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