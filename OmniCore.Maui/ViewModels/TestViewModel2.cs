using System.Diagnostics;
using System.Windows.Input;
using OmniCore.Common.Api;
using OmniCore.Framework.Omnipod.Requests;
using OmniCore.Services;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Amqp;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Platform;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Maui.ViewModels;
public class TestViewModel2 : BaseViewModel
{
    private IAppConfiguration _appConfiguration;
    private IPlatformService _platformService;
    private IPlatformInfo _platformInfo;
    private IAmqpService _amqpService;
    private IApiClient _apiClient;
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
    }
}