using System.Diagnostics;
using System.Windows.Input;
using Dapper;
using OmniCore.Common.Api;
using OmniCore.Services;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Amqp;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Platform;

namespace OmniCore.Maui.ViewModels
{
    public class TestViewModel : BaseViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public ICommand SchlemmCommand { get; set; }
        public ICommand FiskCommand { get; set; }
        public ICommand CheckPermissionsCommand { get; set; }
        
        private IConfigurationStore _configurationStore;
        private IPlatformService _platformService;
        private IPlatformInfo _platformInfo;
        private IAmqpService _amqpService;
        public TestViewModel(IConfigurationStore configurationStore,
            IPlatformService platformService,
            IPlatformInfo platformInfo,
            IAmqpService amqpService)
        {
            _configurationStore = configurationStore;
            _platformService = platformService;
            _platformInfo = platformInfo;
            _amqpService = amqpService;
            SchlemmCommand = new Command(async () => await ExecuteGo());
            FiskCommand = new Command(async () => await ExecuteStop());
            CheckPermissionsCommand = new Command(async () => await CheckPermissions());
        }

        private async Task CheckPermissions()
        {
            await _platformInfo.VerifyPermissions();
        }
        private async Task ExecuteGo()
        {
            var cc = await _configurationStore.GetConfigurationAsync();
            using (var ac = new ApiClient(_configurationStore))
            {
                if (!cc.ClientId.HasValue)
                {
                    Debug.WriteLine($"Client registration");
                    await ac.AuthorizeAccountAsync(Email, Password);
                    await ac.RegisterClientAsync();
                }
            
                Debug.WriteLine($"ClientId: {cc.ClientId}");
                var erp = await ac.GetClientEndpointAsync();
                _amqpService.Dsn = erp.dsn;
                _amqpService.Exchange = erp.exchange;
                _amqpService.Queue = erp.queue;
                _amqpService.UserId = erp.user_id;
            }
            
            _platformService.StartService();
        }

        private async Task ExecuteStop()
        {
            _platformService.StopService();
        }
    }
}