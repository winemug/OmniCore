using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OmniCore.Common.Data;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Pod;
using static OmniCore.Common.Data.OcdbContext;

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

        try
        {
            //await TransferDb();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex.Message}");
        }

        await RadioService.Start();
        await PodService.Start();
        await SyncService.Start();
        await RaddService.Start();
        await AmqpService.Start();
        Debug.WriteLine("Core services started");
    }

    public async Task Stop()
    {
        Debug.WriteLine("Core services stopping");
        await AmqpService.Stop();
        await RaddService.Stop();
        await SyncService.Stop();
        await PodService.Stop();
        await RadioService.Stop();
        Debug.WriteLine("Core services stopped");
    }
}
