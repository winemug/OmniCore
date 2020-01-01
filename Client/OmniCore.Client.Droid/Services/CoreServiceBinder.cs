using Android.Bluetooth;
using Android.OS;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client.Droid.Services
{
    public class CoreServiceBinder : Binder
    {
        public ICoreServices AndroidService { get; }
        public CoreServiceBinder(CoreAndroidService androidServiceImplementation)
        {
            AndroidService = androidServiceImplementation;
        }
    }
}