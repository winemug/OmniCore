using Android.Bluetooth;
using Android.OS;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client.Droid.Platform
{
    public class AndroidServiceBinder : Binder
    {
        public ICoreServiceApi ServiceApi { get; }
        public AndroidServiceBinder(ICoreServiceApi serviceApi)
        {
            ServiceApi = serviceApi;
        }
    }
}