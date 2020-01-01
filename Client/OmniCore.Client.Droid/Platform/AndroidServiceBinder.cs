using Android.Bluetooth;
using Android.OS;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Client.Droid.Platform
{
    public class AndroidServiceBinder : Binder
    {
        public ICoreServices CoreServices { get; }
        public AndroidServiceBinder(ICoreServices coreServices)
        {
            CoreServices = coreServices;
        }
    }
}