using Android.Bluetooth;
using Android.OS;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client.Droid.Services
{
    public class DroidCoreServiceBinder : Binder
    {
        public ICoreServices Services { get; }
        public DroidCoreServiceBinder(DroidCoreService serviceImplementation)
        {
            Services = serviceImplementation;
        }
    }
}