using Android.Content;
using Android.OS;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Platform;

namespace OmniCore.Mobile.Droid
{
    public class ForegroundServiceHelper : IForegroundServiceHelper
    {
        private readonly Context _context;

        public ForegroundServiceHelper(Context context)
        {
            _context = context;
        }
        public void StartForegroundService()
        {
            var intent = new Intent(_context, typeof(ForegroundService));
            intent.SetAction("start");
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                _context.StartForegroundService(intent);
            else
                _context.StartService(intent);
        }

        public void StopForegroundService()
        {
            var intent = new Intent(_context, typeof(ForegroundService));
            intent.SetAction("stop");
            _context.StartService(intent);
        }
    }
}