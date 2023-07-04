using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nito.AsyncEx;
using OmniCore.Client.Model;
using OmniCore.Common.Amqp;
using OmniCore.Common.Core;

namespace OmniCore.Framework;

public class SyncService : ISyncService
{
    private IAmqpService _amqpService;
    private Task _syncTask;
    private CancellationTokenSource _ctsSync;
    private AsyncAutoResetEvent _syncTriggerEvent;

    public event EventHandler<bool> ReadyStateChanged;

    public bool ServiceReady => throw new NotImplementedException();

    public SyncService(
        IAmqpService amqpService)
    {
        _amqpService = amqpService;
        _ctsSync = new CancellationTokenSource();
        _syncTriggerEvent = new AsyncAutoResetEvent(true);
    }
    public async Task Start()
    {
        _syncTask = SyncTask(_ctsSync.Token);
    }

    public async Task Stop()
    {
        _ctsSync.Cancel();
        if (_syncTask != null)
        {
            try
            {
                await _syncTask;
            }
            catch (TaskCanceledException)
            {
            }
        }
    }

    private async Task SyncTask(CancellationToken cancellationToken)
    {
        while(true)
        {
            await _syncTriggerEvent.WaitAsync(cancellationToken);
            await using var context = new OcdbContext();
            var podsToSync = await context.Pods.Where(p => !p.IsSynced).ToListAsync();
            var podActionsToSync = await context.PodActions.Where(pa => !pa.IsSynced).ToListAsync();
            await context.DisposeAsync();

            foreach (var pod in podsToSync)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _amqpService.PublishMessage(new AmqpMessage
                {
                    Text = JsonSerializer.Serialize(new
                    {
                        type = nameof(Pod),
                        data = pod
                    }),
                    Route = "sync",
                    //OnPublishConfirmed = OnPodSynced(pod.PodId),
                });
                await Task.Yield();
            }

            foreach (var podAction in podActionsToSync)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _amqpService.PublishMessage(new AmqpMessage
                {
                    Text = JsonSerializer.Serialize(new
                    {
                        type = nameof(PodAction),
                        data = podAction
                    }),
                    Route = "sync",
                    //OnPublishConfirmed = OnPodActionSynced(podAction.PodId, podAction.Index),
                });
                await Task.Yield();
            }
        }
    }

    public void TriggerSync()
    {
        _syncTriggerEvent.Set();
    }

    private async Task OnPodSynced(Guid podId)
    {
        await using var context = new OcdbContext();
        var pod = await context.Pods.FirstOrDefaultAsync(p => p.PodId == podId);
        if (pod != null)
        {
            pod.IsSynced = true;
            await context.SaveChangesAsync();
        }
    }

    private async Task OnPodActionSynced(Guid podId, int index)
    {
        await using var context = new OcdbContext();
        var podAction = await context.PodActions.FirstOrDefaultAsync(pa => pa.PodId == podId && pa.Index == index);
        if (podAction != null)
        {
            podAction.IsSynced = true;
            await context.SaveChangesAsync();
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}