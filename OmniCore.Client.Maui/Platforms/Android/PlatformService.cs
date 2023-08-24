#if ANDROID
using Android.Content;
#endif
using Android.OS;
using OmniCore.Common.Platform;

namespace OmniCore.Maui.Services
{
    public class PlatformService : IPlatformService
    {
        public void StartService()
        {
            var activity = Android.App.Application.Context;
            var intent = new Intent(activity, typeof(AndroidForegroundService));
            intent.SetAction("start");

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                activity.StartForegroundService(intent);
            else
                activity.StartService(intent);
        }

        public void StopService()
        {
            var activity = Android.App.Application.Context;
            var intent = new Intent(activity, typeof(AndroidForegroundService));
            intent.SetAction("stop");
            activity.StartService(intent);
        }
    }
}