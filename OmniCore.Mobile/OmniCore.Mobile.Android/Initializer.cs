using Android.App;
using Android.Content;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Platform;
using Unity;
using Xamarin.Forms;

namespace OmniCore.Mobile.Droid
{
    public static class Initializer
    {
        public static void RegisterTypesForAndroid(Activity activity)
        {
            var container = new UnityContainer();
            DependencyService.RegisterSingleton<IUnityContainer>(container);

            container.RegisterInstance<IPlatformInfo>(new PlatformInfo(activity));
            container.RegisterInstance<IForegroundServiceHelper>(new ForegroundServiceHelper(activity));
        }
    }
}