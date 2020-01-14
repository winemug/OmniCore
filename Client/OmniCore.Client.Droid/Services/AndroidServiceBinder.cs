using Android.Bluetooth;
using Android.OS;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Client.Droid.Services
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