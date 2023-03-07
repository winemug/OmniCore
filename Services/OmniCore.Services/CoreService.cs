using System.Diagnostics;
using System.Threading.Tasks;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Core;
using Unity;

namespace OmniCore.Services;

public class CoreService : ICoreService
{
    [Dependency] public IRadioService RadioService { get; set; }

    [Dependency] public IAmqpService AmqpService { get; set; }

    [Dependency] public IPodService PodService { get; set; }

    [Dependency] public IDataService DataService { get; set; }
    
    [Dependency] public IConfigurationService ConfigurationService { get; set; }

    public async Task Start()
    {
        Debug.WriteLine("Core services starting");

        await Task.WhenAll(
            ConfigurationService.Start(),
            DataService.Start()
            );
        await Task.WhenAll(
            RadioService.Start(),
            PodService.Start(),
            AmqpService.Start()
        );
        Debug.WriteLine("Core services started");
    }

    public async Task Stop()
    {
        Debug.WriteLine("Core services stopping");
        await Task.WhenAll(
            RadioService.Stop(),
            PodService.Stop(),
            AmqpService.Stop()
        );
        await Task.WhenAll(
            ConfigurationService.Stop(),
            DataService.Stop()
        );
        Debug.WriteLine("Core services stopped");
    }
}