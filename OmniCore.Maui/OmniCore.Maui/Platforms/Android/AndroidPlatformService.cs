using Android.Content;
using Android.OS;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Platform;

namespace OmniCore.Maui
{
    public class AndroidPlatformService : IPlatformService
    {
        private readonly Context _context;

        public AndroidPlatformService()
        {
            _context = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        }
        public void StartService()
        {
            var intent = new Intent(_context, typeof(AndroidForegroundService));
            intent.SetAction("start");
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                _context.StartForegroundService(intent);
            else
                _context.StartService(intent);
        }

        public void StopService()
        {
            var intent = new Intent(_context, typeof(AndroidForegroundService));
            intent.SetAction("stop");
            _context.StartService(intent);
        }
    }
}