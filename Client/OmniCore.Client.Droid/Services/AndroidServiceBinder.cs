using Android.OS;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client.Droid.Services
{
    public class AndroidServiceBinder : Binder
    {
        public AndroidServiceBinder(IApi api)
        {
            Api = api;
        }
        public IApi Api { get; }
    }
}