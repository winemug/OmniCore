using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Nito.AsyncEx;
using OmniCore.Client.Model;
using OmniCore.Common.Amqp;
using OmniCore.Common.Core;

namespace OmniCore.Framework;

public class SyncService : BackgroundService, ISyncService
{
    private readonly IAmqpService _amqpService;
    private readonly AsyncAutoResetEvent _syncTriggerEvent;

    public SyncService(
        IAmqpService amqpService)
    {
        _amqpService = amqpService;
        _syncTriggerEvent = new AsyncAutoResetEvent(true);
        _amqpService.RegisterMessageHandler(HandleMessageAsync);
    }

    public void TriggerSync()
    {
        _syncTriggerEvent.Set();
    }

    private async Task<bool> HandleMessageAsync(AmqpMessage message)
    {
        return false;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _syncTriggerEvent.WaitAsync(stoppingToken);

        // while(true)
        // {
        //     await using var context = new OcdbContext();
        //     var podsToSync = await context.Pods.Where(p => !p.IsSynced).ToListAsync(stoppingToken);
        //     var podActionsToSync = await context.PodActions.Where(pa => !pa.IsSynced)
        //         .ToListAsync(stoppingToken);
        //     await context.DisposeAsync();
        //
        //     foreach (var pod in podsToSync)
        //     {
        //         stoppingToken.ThrowIfCancellationRequested();
        //         _amqpService.PublishMessage(new AmqpMessage
        //         {
        //             Text = JsonSerializer.Serialize(new
        //             {
        //                 type = nameof(Pod),
        //                 data = pod
        //             }),
        //             Route = "sync",
        //             WhenPublished = () => OnPodSynced(pod.PodId),
        //         });
        //         await Task.Yield();
        //     }
        //
        //     foreach (var podAction in podActionsToSync)
        //     {
        //         stoppingToken.ThrowIfCancellationRequested();
        //         _amqpService.PublishMessage(new AmqpMessage
        //         {
        //             Text = JsonSerializer.Serialize(new
        //             {
        //                 type = nameof(PodAction),
        //                 data = podAction
        //             }),
        //             Route = "sync",
        //             WhenPublished = () => OnPodActionSynced(podAction.PodId, podAction.Index),
        //         });
        //         await Task.Yield();
        //     }
        // }
    }

    private async Task OnPodSynced(Guid podId)
    {
        await using var context = new OcDbContext();
        var pod = await context.Pods.FirstOrDefaultAsync(p => p.PodId == podId);
        if (pod != null)
        {
            pod.IsSynced = true;
            await context.SaveChangesAsync();
        }
    }

    private async Task OnPodActionSynced(Guid podId, int index)
    {
        await using var context = new OcDbContext();
        var podAction = await context.PodActions.FirstOrDefaultAsync(pa => pa.PodId == podId && pa.Index == index);
        if (podAction != null)
        {
            podAction.IsSynced = true;
            await context.SaveChangesAsync();
        }
    }
}