using Android.App;
using Android.Content;
using OmniCore.Services.Interfaces;
using Xamarin.Forms;

namespace OmniCore.Mobile.Droid
{
    public static class Initializer
    {
        public static void RegisterTypesForAndroid(Activity activity)
        {
            DependencyService.RegisterSingleton<IPlatformInfo>(new PlatformInfo(activity));
            DependencyService.RegisterSingleton<IForegroundServiceHelper>(new ForegroundServiceHelper(activity));
        }
    }
}