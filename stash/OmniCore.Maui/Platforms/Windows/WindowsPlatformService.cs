using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Platform;

namespace OmniCore.Maui
{
    public class WindowsPlatformService : IPlatformService
    {
        private bool _started;
        private ICoreService _coreService;

        public WindowsPlatformService(ICoreService coreService)
        {
            _coreService = coreService;
        }
        public async void StartService()
        {
            if (!_started)
            {
                await _coreService.Start();
            }
            _started = true;
        }

        public async void StopService()
        {
            if (_started)
            {
                await _coreService.Start();
            }
            _started = false;
        }
    }
}