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
    private IDataService _dataService;
    private IAmqpService _amqpService;
    private IConfigurationStore _configurationStore;
    
    public SyncService(
        IDataService dataService,
        IAmqpService amqpService,
        IConfigurationStore configurationStore)
    {
        _dataService = dataService;
        _amqpService = amqpService;
        _configurationStore = configurationStore;
    }
    public async Task Start()
    {
        var cc = await _configurationStore.GetConfigurationAsync();
        var conn = await _dataService.GetConnectionAsync();

        // await conn.ExecuteAsync("UPDATE pod SET synced = 0");
        // await conn.ExecuteAsync("UPDATE pod_message SET synced = 0");
        
        var pods = await conn.QueryAsync("SELECT * FROM pod WHERE synced = 0");
        foreach (var pod in pods)
        {
            var msg = new AmqpMessage
            {
                Text = JsonSerializer.Serialize(new
                {
                    type="pod",
                    data=pod
                }),
                Route = "sync",
                OnPublishConfirmed = async (msg) => await OnPodSynced(pod.id),
                // Headers = new Dictionary<string, object>
                // {
                //     { "table", "pod" }
                // }
            };
            await _amqpService.PublishMessage(msg);
        }

#if DEBUG

        //using var ocdb = new OcdbContext();
        //foreach (var pod in ocdb.Pods.Where(p => !p.IsSynced))
        //{
        //    var msg = new AmqpMessage
        //    {
        //        Text = JsonSerializer.Serialize(new
        //        {
        //            type = nameof(Pod),
        //            data = pod
        //        }),
        //        Route = "sync",
        //        OnPublishConfirmed = async (msg) => await OnPodSynced(pod),
        //    };
        //    await _amqpService.PublishMessage(msg);
        //}
#endif

        var podMessages = await conn.QueryAsync("SELECT * FROM pod_message WHERE synced = 0");
        foreach (var podMessage in podMessages)
        {
            var msg = new AmqpMessage
            {
                Text = JsonSerializer.Serialize(new
                {
                    type="pod_message",
                    data=podMessage
                }),
                Route = "sync",
                OnPublishConfirmed = async (msg) => await OnPodMessageSynced(podMessage.pod_id,
                    (int)podMessage.record_index),
                // Headers = new Dictionary<string, object>
                // {
                //     { "table", "pod" }
                // }
            };
            await _amqpService.PublishMessage(msg);
        }

#if DEBUG
        //foreach (var podAction in ocdb.PodActions.Where(pa => !pa.IsSynced))
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
        //        // Headers = new Dictionary<string, object>
        //        // {
        //        //     { "table", "pod" }
        //        // }
        //    };
        //    await _amqpService.PublishMessage(msg);
        //}
#endif
    }

    public async Task Stop()
    {
    }

    private async Task OnPodSynced(string podId)
    {
        try
        {
            var conn = await _dataService.GetConnectionAsync();
            await conn.ExecuteAsync("UPDATE pod SET synced = 1 WHERE id=@id",
                new
                {
                    id = podId
                });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
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

    private async Task OnPodMessageSynced(string podId, int index)
    {
        try
        {
            var conn = await _dataService.GetConnectionAsync();
            await conn.ExecuteAsync("UPDATE pod_message SET synced = 1 WHERE pod_id=@podId AND record_index=@idx",
                new
                {
                    podId = podId,
                    idx=index
                });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task SyncPodMessage(Guid podId, int recordIndex)
    {
        var conn = await _dataService.GetConnectionAsync();
        var podMessage = await conn.QueryFirstOrDefaultAsync("SELECT * FROM pod_message WHERE pod_id=@id" +
                                                      " AND record_index=@ri",
            new { id=podId.ToString("N"), ri=recordIndex});
        if (podMessage == null)
            return;
        
        var msg = new AmqpMessage
        {
            Text = JsonSerializer.Serialize(new
            {
                type="pod_message",
                data=podMessage
            }),
            Route = "sync",
            OnPublishConfirmed = async (_) => await OnPodMessageSynced(podId.ToString("N"), recordIndex),
        };
        await _amqpService.PublishMessage(msg);

#if DEBUG
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
#endif
    }

    public async Task SyncPod(Guid podId)
    {
        var conn = await _dataService.GetConnectionAsync();
        var pod = await conn.QueryFirstOrDefaultAsync("SELECT * FROM pod WHERE id=@id", new { id=podId.ToString("N")});
        if (pod == null)
            return;
        
        var msg = new AmqpMessage
        {
            Text = JsonSerializer.Serialize(new
            {
                type="pod",
                data=pod
            }),
            Route = "sync",
            OnPublishConfirmed = async (_) => await OnPodSynced(pod.id),
        };
        await _amqpService.PublishMessage(msg);

#if DEBUG
        //var ocdb = new OcdbContext();
        //var p = ocdb.Pods.FirstOrDefault(p => p.PodId == podId);
        //if (p != null)
        //{
        //    await _amqpService.PublishMessage(new AmqpMessage
        //    {
        //        Text = JsonSerializer.Serialize(new
        //        {
        //            type = nameof(OmniCore.Common.Data.Pod),
        //            data = p
        //        }),
        //        Route = "sync",
        //        OnPublishConfirmed = async (_) => await OnPodSynced(p)
        //    });
        //}
#endif
    }
}