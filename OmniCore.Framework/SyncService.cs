using System.Diagnostics;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Dapper;
using OmniCore.Services.Interfaces.Amqp;
using OmniCore.Services.Interfaces.Core;

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
    }
}