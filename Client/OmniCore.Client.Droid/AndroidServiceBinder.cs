using Android.Bluetooth;
using Android.OS;
using OmniCore.Model.Interfaces.Common;

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