using System.Diagnostics;
using OmniCore.Services.Interfaces;
using Unity;

namespace OmniCore.Services;

public class ForegroundService : IForegroundService
{
    [Dependency] public IRadioService RadioService { get; set; }

    [Dependency] public IAmqpService AmqpService { get; set; }

    [Dependency] public IPodService PodService { get; set; }

    [Dependency] public IDataService DataService { get; set; }

    public void Start()
    {
        Debug.WriteLine("Core services starting");
        DataService.Start();
        RadioService.Start();
        PodService.Start();
        AmqpService.Start();
        Debug.WriteLine("Core services started");
    }

    public void Stop()
    {
        Debug.WriteLine("Core services stopping");
        AmqpService.Stop();
        PodService.Stop();
        RadioService.Stop();
        DataService.Stop();
        Debug.WriteLine("Core services stopped");
    }
}