using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client.Droid.Services
{
    public class DroidCoreServiceConnection : Java.Lang.Object, IServiceConnection, ICoreServicesProvider
    {
        
        public DroidCoreServiceConnection(ICoreServices localServices)
        {
            LocalServices = localServices;
        }

        public ICoreServices Services => Binder?.Services;

        private DroidCoreServiceBinder Binder;
        
        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            Binder = service as DroidCoreServiceBinder;
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            Binder = null;
        }

        public ICoreServices LocalServices { get; }

        public async Task<ICoreServices> GetRemoteServices(ICoreServicesDescriptor serviceDescriptor, ICoreCredentials credentials)
        {
            return null;
        }

        public async Task<IAsyncEnumerable<ICoreServicesDescriptor>> ListRemoteServices()
        {
            return null;
        }
    }
}