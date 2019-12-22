using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client.Droid.Services
{
    public class DroidCoreServiceConnection : Java.Lang.Object, IServiceConnection
    {
        
        public bool IsConnected { get; private set; }
        public DroidCoreServiceBinder Binder { get; private set; }
        
        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            Binder = service as DroidCoreServiceBinder;
            IsConnected = this.Binder != null;
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            IsConnected = false;
            Binder = null;
        }
    }
}