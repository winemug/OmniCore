using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OmniCore.Common.Data;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Core;

namespace OmniCore.Services;

public class CoreService : ICoreService
{
    public IRadioService RadioService { get; set; }
    public IAmqpService AmqpService { get; set; }
    public IPodService PodService { get; set; }
    public ISyncService SyncService { get; set; }
    public IRaddService RaddService { get; set; }
    public CoreService(IRadioService radioService,
        IAmqpService amqpService,
        IPodService podService,
        ISyncService syncService,
        IRaddService raddService)
    {
        RadioService = radioService;
        AmqpService = amqpService;
        PodService = podService;
        SyncService = syncService;
        RaddService = raddService;
    }

    public async Task Start()
    {
        Debug.WriteLine("Core services starting");

        using (var ocdbContext = new OcdbContext())
        {
            await ocdbContext.TransferDb();
        }
        
        await Task.WhenAll(
            RadioService.Start(),
            PodService.Start(),
            AmqpService.Start(),
            SyncService.Start(),
            RaddService.Start()
        );
        Debug.WriteLine("Core services started");
    }

    public async Task Stop()
    {
        Debug.WriteLine("Core services stopping");
        await Task.WhenAll(
            SyncService.Stop(),
            RaddService.Stop(),
            AmqpService.Stop(),
            PodService.Stop(),
            RadioService.Stop()
        );
        Debug.WriteLine("Core services stopped");
    }
}
