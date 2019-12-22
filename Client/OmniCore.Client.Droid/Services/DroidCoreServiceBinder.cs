using Android.OS;

namespace OmniCore.Client.Droid.Services
{
    public class DroidCoreServiceBinder : Binder
    {
        private readonly DroidCoreService ServiceImplementation;
        public DroidCoreServiceBinder(DroidCoreService serviceImplementation)
        {
            ServiceImplementation = serviceImplementation;
        }
    }
}