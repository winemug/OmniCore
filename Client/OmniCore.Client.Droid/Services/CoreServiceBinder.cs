using Android.Bluetooth;
using Android.OS;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client.Droid.Services
{
    public class CoreServiceBinder : Binder
    {
        public ICoreBootstrapper Bootstrapper { get; }
        public CoreServiceBinder(CoreBootstrapper bootstrapperImplementation)
        {
            Bootstrapper = bootstrapperImplementation;
        }
    }
}