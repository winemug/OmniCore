using Android.Bluetooth;
using Android.OS;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Client.Droid.Services
{
    public class AndroidServiceBinder : Binder
    {
        public AndroidService AndroidService { get; }
        public AndroidServiceBinder(AndroidService androidServiceImplementation)
        {
            AndroidService = androidServiceImplementation;
        }
    }
}