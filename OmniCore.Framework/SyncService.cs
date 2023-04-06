using System.Diagnostics;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Dapper;
using OmniCore.Common.Data;
using OmniCore.Services.Interfaces.Amqp;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class SyncService : ISyncService
{
    private IAmqpService _amqpService;
    private IConfigurationStore _configurationStore;
    
    public SyncService(
        IAmqpService amqpService,
        IConfigurationStore configurationStore)
    {
        _amqpService = amqpService;
        _configurationStore = configurationStore;
    }
    public async Task Start()
    {
        var cc = await _configurationStore.GetConfigurationAsync();
        using var ocdb = new OcdbContext();
        foreach (var pod in ocdb.Pods.Where(p => !p.IsSynced))
        {
            var msg = new AmqpMessage
            {
                Text = JsonSerializer.Serialize(new
                {
                    type="Pod",
                    data=pod
                }),
                Route = "sync",
                OnPublishConfirmed = async (msg) => await OnPodSynced(pod),
            };
            await _amqpService.PublishMessage(msg);
        }
        
        foreach (var podAction in ocdb.PodActions.Where(pa => !pa.IsSynced))
        {
            var msg = new AmqpMessage
            {
                Text = JsonSerializer.Serialize(new
                {
                    type = nameof(PodAction),
                    data = podAction
                }),
                Route = "sync",
                OnPublishConfirmed = async (msg) => await OnPodActionSynced(podAction),
            };
            await _amqpService.PublishMessage(msg);
        }
    }

    public async Task Stop()
    {
    }

    private async Task OnPodSynced(OmniCore.Common.Data.Pod pod)
    {
        using var ocdb = new OcdbContext();
        pod.IsSynced = true;
        ocdb.Update<OmniCore.Common.Data.Pod>(pod);
        await ocdb.SaveChangesAsync();
    }

    private async Task OnPodActionSynced(PodAction podAction)
    {
        using var ocdb = new OcdbContext();
        podAction.IsSynced = true;
        ocdb.Update<PodAction>(podAction);
        await ocdb.SaveChangesAsync();
    }

    public async Task SyncPodMessage(Guid podId, int recordIndex)
    {
        var ocdb = new OcdbContext();
        var pa = ocdb.PodActions.FirstOrDefault(pa => pa.PodId == podId && pa.Index == recordIndex);
        if (pa != null)
        {
            await _amqpService.PublishMessage(new AmqpMessage
            {
                Text = JsonSerializer.Serialize(new
                {
                    type = nameof(PodAction),
                    data = pa
                }),
                Route = "sync",
                OnPublishConfirmed = async (_) => await OnPodActionSynced(pa)
            });
        }
    }

    public async Task SyncPod(Guid podId)
    {
        var ocdb = new OcdbContext();
        var p = ocdb.Pods.FirstOrDefault(p => p.PodId == podId);
        if (p != null)
        {
            await _amqpService.PublishMessage(new AmqpMessage
            {
                Text = JsonSerializer.Serialize(new
                {
                    type = nameof(OmniCore.Common.Data.Pod),
                    data = p
                }),
                Route = "sync",
                OnPublishConfirmed = async (_) => await OnPodSynced(p)
            });
        }
    }
}