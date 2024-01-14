using System.Diagnostics;
using System.Windows.Input;
using OmniCore.Common.Api;
using OmniCore.Common.Data;
using OmniCore.Framework.Omnipod.Requests;
using OmniCore.Services;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Amqp;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Platform;
using OmniCore.Services.Interfaces.Pod;
using OmniCore.Shared.Api;
using Org.Apache.Http.Client.Params;

namespace OmniCore.Maui.ViewModels;
public class TestViewModel : BaseViewModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string ClientName { get; set; }
    public ICommand NewPodCommand { get; set; }
    public ICommand PrimeCommand { get; set; }
    public ICommand StartCommand { get; set; }
    public ICommand StopCommand { get; set; }

    private IPlatformService _platformService;
    private IPlatformInfo _platformInfo;
    private IAmqpService _amqpService;
    private IApiClient _apiClient;
    private IPodService _podService;
    private IRadioService _radioService;
    private IAppConfiguration _appConfiguration;

    public TestViewModel(
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

        StopCommand = new Command(async () => await ExecuteStop());
        NewPodCommand = new Command(async () => await ExecuteNewPod());
        PrimeCommand = new Command(async () => await ExecutePrime());
        StartCommand = new Command(async () => await ExecuteStartPod());

        if (_appConfiguration.AccountEmail == null)
        {
            _appConfiguration.AccountEmail = "barisk@gmail.com";
        }

        if (!_appConfiguration.AccountVerified)
        {
            _appConfiguration.AccountVerified = true;
        }

        if (_appConfiguration.ClientAuthorization == null)
        {
            _appConfiguration.ClientAuthorization = new ClientAuthorization
            {
                ClientId = new Guid("EE843C96-A312-4D4B-B0CC-93E22D6E680E"),
                Token = new byte[16]
            };
        }
    }

    public override async ValueTask OnAppearing()
    {
        using var context = new OcdbContext();
        await context.Database.EnsureCreatedAsync();
        _platformService.StartService();
    }
 
    private async Task ExecuteStop()
    {
        _platformService.StopService();
    }

    private async Task ExecuteNewPod()
    {
        var podId = await _podService.NewPodAsync(new Guid("7D799596-3F6D-48E2-AC65-33CA6396788B"), 100, MedicationType.Insulin);
        // var pods = await _podService.GetPodsAsync();
        // var pod = pods[1];
        // using (var pc = await _podService.GetConnectionAsync(pod))
        // {
        //     await pc.Deactivate();
        // }
    }
    
    private async Task ExecutePrime()
    {
        var pods = await _podService.GetPodsAsync();
        var pod = pods[0];
        using (var pc = await _podService.GetConnectionAsync(pod))
        {
            var now = DateTime.Now;
            var res = await pc.PrimePodAsync(new DateOnly(now.Year, now.Month, now.Day),
                new TimeOnly(now.Hour, now.Minute, now.Second),
                true, CancellationToken.None);
        }
    }

    private async Task ExecuteStartPod()
    {
        var pods = await _podService.GetPodsAsync();
        var pod = pods[0];
        using (var pc = await _podService.GetConnectionAsync(pod))
        {
            var now = DateTime.Now;
            var basalRateTicks = new int[48];
            for (int i = 0; i < 48; i++)
                basalRateTicks[i] = 12;

            var res = await pc.StartPodAsync(
                new TimeOnly(now.Hour, now.Minute, now.Second), basalRateTicks);
        }
    }

}