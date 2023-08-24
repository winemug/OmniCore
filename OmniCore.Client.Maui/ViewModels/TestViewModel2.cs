using System.Windows.Input;
using OmniCore.Common.Api;
using OmniCore.Common.Core;
using OmniCore.Common.Platform;
using OmniCore.Shared.Api;

namespace OmniCore.Maui.ViewModels;

public class TestViewModel2
{
    private IAmqpService _amqpService;
    private readonly IApiClient _apiClient;
    private readonly IAppConfiguration _appConfiguration;
    private readonly IPlatformInfo _platformInfo;
    private readonly IPlatformService _platformService;
    private IPodService _podService;
    private IRadioService _radioService;

    public TestViewModel2(
        IAppConfiguration appConfiguration,
        IPlatformService platformService,
        IPlatformInfo platformInfo,
        IAmqpService amqpService,
        IApiClient apiClient,
        IPodService podService,
        IRadioService radioService)
    {
        _appConfiguration = appConfiguration;
        _platformService = platformService;
        _platformInfo = platformInfo;
        _amqpService = amqpService;
        _apiClient = apiClient;
        _podService = podService;
        _radioService = radioService;
        TestCommand1 = new Command(async () => ExecuteTestCommand1());
        TestCommand2 = new Command(async () => ExecuteTestCommand2());
    }

    public ICommand TestCommand1 { get; set; }
    public ICommand TestCommand2 { get; set; }

    public async void ExecuteTestCommand2()
    {
        _appConfiguration.AccountEmail = null;
        _appConfiguration.AccountVerified = false;
        _appConfiguration.ClientAuthorization = null;
    }

    public async void ExecuteTestCommand1()
    {
        await _platformInfo.VerifyPermissions(false);

        var email = "gggggg@balya.net";
        var password = "jjjj";
        var code = "0000";
        var clientName = "bbbb";

        if (_appConfiguration.AccountEmail == null)
        {
            var result = await _apiClient.PostRequestAsync<AccountRegistrationRequest, ApiResponse>(
                Routes.AccountRegistrationRequestRoute, new AccountRegistrationRequest
                {
                    Email = email
                });
            if (result is not { Success: true })
                return;
            _appConfiguration.AccountEmail = email;
        }

        if (!_appConfiguration.AccountVerified)
        {
            var result = await _apiClient.PostRequestAsync<AccountVerificationRequest, ApiResponse>(
                Routes.AccountVerificationRequestRoute, new AccountVerificationRequest
                {
                    Email = _appConfiguration.AccountEmail,
                    Password = password,
                    Code = code
                });
            if (result is not { Success: true })
                return;

            _appConfiguration.AccountVerified = true;
        }

        if (_appConfiguration.ClientAuthorization == null)
        {
            var result = await _apiClient.PostRequestAsync<ClientRegistrationRequest, ClientRegistrationResponse>(
                Routes.ClientRegistrationRequestRoute, new ClientRegistrationRequest
                {
                    Email = _appConfiguration.AccountEmail,
                    Password = password,
                    ClientName = clientName
                });
            if (result is not { Success: true })
                return;

            _appConfiguration.ClientAuthorization = new ClientAuthorization
            {
                ClientId = result.ClientId,
                Token = result.Token
            };
        }

        _platformService.StartService();
    }
}