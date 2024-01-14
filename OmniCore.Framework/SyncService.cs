using System.Diagnostics;
using System.Dynamic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Nito.AsyncEx;
using OmniCore.Common.Data;
using OmniCore.Common.Pod;
using OmniCore.Services.Interfaces.Amqp;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Pod;
using Polly;

namespace OmniCore.Services;

public class SyncService : ISyncService
{
    private IAmqpService _amqpService;
    private Task _syncTask;
    private CancellationTokenSource _ctsSync;
    private AsyncAutoResetEvent _syncTriggerEvent;
    public SyncService(
        IAmqpService amqpService,
        OcdbContext ocdbContext)
    {
        _amqpService = amqpService;
        _ctsSync = new CancellationTokenSource();
        _syncTriggerEvent = new AsyncAutoResetEvent(true);
    }
    public async Task Start()
    {
        _syncTask = SyncTask(_ctsSync.Token);
        _amqpService.RegisterMessageProcessor(ProcessMessageAsync);
    }

    public async Task<bool> ProcessMessageAsync(AmqpMessage message)
    {
        //Debug.WriteLine($"Message: {message}");
        try
        {
            switch (message.Type)
            {
                case "Pod":
                    //Debug.Write($"Type: {syncMessage.Type} ");
                    await SyncPod(JsonSerializer.Deserialize<PodSync>(message.Text));
                    //Debug.WriteLine($"OK");
                    return true;
                case "PodAction":
                    //Debug.Write($"Type: {syncMessage.Type} ");
                    //return false;
                    await SyncAction(JsonSerializer.Deserialize<PodActionSync>(message.Text));
                    //Debug.WriteLine($"OK");
                    return true;
                default:
                    throw new ApplicationException($"Unknown type");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Message: {message} parsing FAILED\n");
            return false;
        }
    }

    private async Task SyncPod(PodSync mpod)
    {
        using var context = new OcdbContext();
        var pod = await context.Pods.Where(p => p.PodId == mpod.PodId).FirstOrDefaultAsync();
        if (pod != null)
        {
            pod.ClientId = mpod.ClientId;
            pod.ProfileId = mpod.ProfileId;
            pod.Medication = (MedicationType)mpod.Medication;
            pod.UnitsPerMilliliter = mpod.UnitsPerMilliliter;
            pod.RadioAddress = mpod.RadioAddress;
            pod.Created = mpod.Created;
            pod.Removed = mpod.Removed;
        }
        else
        {
            pod = new Pod
            {
                PodId = mpod.PodId,
                ClientId = mpod.ClientId,
                ProfileId = mpod.ProfileId,
                Created = mpod.Created,
                Medication = (MedicationType)mpod.Medication,
                UnitsPerMilliliter = mpod.UnitsPerMilliliter,
                RadioAddress = mpod.RadioAddress,
                Removed = mpod.Removed
            };
            await context.Pods.AddAsync(pod);
        }

        // if (mpod.Lot.HasValue && mpod.Serial.HasValue)
        //     pod.ImportedProperties = new PodImportedProperties
        //     {
        //         Lot = mpod.Lot.Value,
        //         Serial = mpod.Serial.Value
        //     };

        await context.SaveChangesAsync();
        Debug.WriteLine($"Pod imported: {mpod.PodId}");
    }


    private async Task SyncAction(PodActionSync mpa)
    {
        using var context = new OcdbContext();

        if (await context.PodActions.Where(pa => pa.PodId == mpa.PodId && pa.Index == mpa.Index).AnyAsync())
        {
            System.Console.WriteLine("Ignoring existing action data");
            return;
        }

        var pa = new PodAction
        {
            PodId = mpa.PodId,
            Index = mpa.Index,
            ClientId = mpa.ClientId.Value,
            RequestSentEarliest = mpa.RequestSentEarliest,
            RequestSentLatest = mpa.RequestSentLatest,
            Result = (Shared.Enums.AcceptanceType)mpa.Result,
            SentData = mpa.SentData,
            ReceivedData = mpa.ReceivedData
        };

        await context.PodActions.AddAsync(pa);
        await context.SaveChangesAsync();
        Debug.WriteLine($"Pod action imported: {mpa.PodId} {mpa.Index}");
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
                await _amqpService.PublishMessage(new AmqpMessage
                {
                    Text = JsonSerializer.Serialize(pod),
                    Type = "Pod",
                    Route = "sync",
                    OnPublishConfirmed = OnPodSynced(pod.PodId),
                });
                await Task.Yield();
            }

            foreach (var podAction in podActionsToSync)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _amqpService.PublishMessage(new AmqpMessage
                {
                    Text = JsonSerializer.Serialize(podAction),
                    Type = "PodAction",
                    Route = "sync",
                    OnPublishConfirmed = OnPodActionSynced(podAction.PodId, podAction.Index),
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
}
public class SyncMessage
{
    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("data")] public JsonObject Data { get; set; }
}

public class PodSync
{
    public Guid PodId { get; set; }
    public Guid ProfileId { get; set; }
    public Guid ClientId { get; set; }
    public uint RadioAddress { get; set; }
    public int Medication { get; set; }
    public int UnitsPerMilliliter { get; set; }
    public uint? Lot { get; set; }
    public uint? Serial { get; set; }
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? Removed { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}

public class PodActionSync
{
    public Guid PodId { get; set; }
    public Guid? ClientId { get; set; }
    public int Index { get; set; }
    public DateTimeOffset? RequestSentEarliest { get; set; }
    public byte[]? SentData { get; set; }
    public DateTimeOffset? RequestSentLatest { get; set; }
    public byte[]? ReceivedData { get; set; }
    public int Result { get; set; }
}