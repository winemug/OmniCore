using System.Diagnostics;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Dapper;
using Microsoft.EntityFrameworkCore;
using OmniCore.Common.Data;
using OmniCore.Services.Interfaces.Amqp;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class SyncService : ISyncService
{
    private IAmqpService _amqpService;
    private IConfigurationStore _configurationStore;
    private OcdbContext _ocdbContext;
    
    public SyncService(
        IAmqpService amqpService,
        IConfigurationStore configurationStore,
        OcdbContext ocdbContext)
    {
        _amqpService = amqpService;
        _configurationStore = configurationStore;
        _ocdbContext = ocdbContext;
    }
    public async Task Start()
    {
        var cc = await _configurationStore.GetConfigurationAsync();

        var podsToSync = _ocdbContext.Pods.Where(p => !p.IsSynced).ToList();
        var podActionsToSync = _ocdbContext.PodActions.Where(pa => !pa.IsSynced).ToList();
        //foreach (var pod in podsToSync)
        //{
        //    var msg = new AmqpMessage
        //    {
        //        Text = JsonSerializer.Serialize(new
        //        {
        //            type="Pod",
        //            data=pod
        //        }),
        //        Route = "sync",
        //        OnPublishConfirmed = async (msg) => await OnPodSynced(pod),
        //    };
        //    await _amqpService.PublishMessage(msg);
        //}
        
        //foreach (var podAction in podActionsToSync)
        //{
        //    var msg = new AmqpMessage
        //    {
        //        Text = JsonSerializer.Serialize(new
        //        {
        //            type = nameof(PodAction),
        //            data = podAction
        //        }),
        //        Route = "sync",
        //        OnPublishConfirmed = async (msg) => await OnPodActionSynced(podAction),
        //    };
        //    await _amqpService.PublishMessage(msg);
        //}
    }

    public async Task Stop()
    {
    }

    private async Task OnPodSynced(Pod pod)
    {
        pod.IsSynced = true;
        await _ocdbContext.SaveChangesAsync();
    }

    private async Task OnPodActionSynced(PodAction podAction)
    {
        podAction.IsSynced = true;
        await _ocdbContext.SaveChangesAsync();
    }

    public async Task SyncPodMessage(Guid podId, int recordIndex)
    {
        //var ocdb = new OcdbContext();
        //var pa = ocdb.PodActions.FirstOrDefault(pa => pa.PodId == podId && pa.Index == recordIndex);
        //if (pa != null)
        //{
        //    await _amqpService.PublishMessage(new AmqpMessage
        //    {
        //        Text = JsonSerializer.Serialize(new
        //        {
        //            type = nameof(PodAction),
        //            data = pa
        //        }),
        //        Route = "sync",
        //        OnPublishConfirmed = async (_) => await OnPodActionSynced(pa)
        //    });
        //}
    }

    public async Task SyncPod(Guid podId)
    {
        //var ocdb = new OcdbContext();
        //var p = ocdb.Pods.FirstOrDefault(p => p.PodId == podId);
        //if (p != null)
        //{
        //    await _amqpService.PublishMessage(new AmqpMessage
        //    {
        //        Text = JsonSerializer.Serialize(new
        //        {
        //            type = nameof(Pod),
        //            data = p
        //        }),
        //        Route = "sync",
        //        OnPublishConfirmed = async (_) => await OnPodSynced(p)
        //    });
        //}
    }
}