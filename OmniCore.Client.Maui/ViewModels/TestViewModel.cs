using System.Diagnostics;
using System.Windows.Input;
using OmniCore.Common.Api;
using OmniCore.Common.Core;
using OmniCore.Common.Platform;
using OmniCore.Shared.Enums;

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
    }

    public override async ValueTask OnAppearing()
    {
        _platformService.StartService();
    }
 
    private async Task ExecuteStop()
    {
        _platformService.StopService();
    }

    private async Task ExecuteNewPod()
    {
        // newPodId = await _podService.NewPodAsync(200, MedicationType.Insulin);
        await _podService.NewPodAsync(
            Guid.Parse("7d799596-3f6d-48e2-ac65-33ca6396788b"),
            200,
            MedicationType.Insulin);
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
                basalRateTicks[i] = 16;

            var res = await pc.StartPodAsync(
                new TimeOnly(now.Hour, now.Minute, now.Second), basalRateTicks);
        }
    }

}