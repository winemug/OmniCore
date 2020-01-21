using Android.Bluetooth;
using Android.OS;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Client.Droid.Services
{
    public class AndroidServiceBinder : Binder
    {
        public ICoreApi Api { get; }
        public AndroidServiceBinder(ICoreApi api)
        {
            Api = api;
        }
    }
}