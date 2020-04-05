using Android.OS;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client.Droid.Services
{
    public class AndroidServiceBinder : Binder
    {
        public AndroidServiceBinder(ICoreApi api)
        {
            Api = api;
        }

        public ICoreApi Api { get; }
    }
}