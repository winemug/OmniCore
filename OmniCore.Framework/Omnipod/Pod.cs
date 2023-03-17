using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Nito.AsyncEx;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class Pod : IPod
{
    private readonly AsyncLock _allocationLock = new();
    private IDataService _dataService;
    private int NonceIndex;

    private uint[] NonceTable;

    public Pod(IDataService dataService)
    {
        _dataService = dataService;
        Info = new PodRuntimeInformation();
        Id = Guid.NewGuid();
        var r = new Random();
        var bn0 = r.Next(13);
        var bn1 = r.Next(16);
        var b0 = ((bn0 + 2) << 4) | bn1;
        var b123 = new byte[3];
        r.NextBytes(b123);
        RadioAddress = (uint)((b0 << 24) | (b123[0] << 16) | (b123[1] << 8) | b123[2]);

        InitializeNonceTable(0);
    }

    public Guid Id { get; set; }
    public uint RadioAddress { get; set; }
    public int UnitsPerMilliliter { get; set; }
    public MedicationType Medication { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }
    public PodRuntimeInformation Info { get; set; }

    public async Task<IDisposable> LockAsync(CancellationToken cancellationToken)
    {
        return await _allocationLock.LockAsync(cancellationToken);
    }

    // public async Task SaveAsync()
    // {
    //     using (var conn = await _dataService.GetConnectionAsync())
    //     {
    //         await conn.ExecuteAsync(
    // "UPDATE pod SET radio_address=@ra, units_per_ml=@upml, medication=@med, lot=@lot, serial=@serial, progress=@pro," +
    // " packet_sequence=@ps, message_sequence=@ms, next_record_index=@ri WHERE id = @id",
    // new
    // {
    //     id = Id.ToString("N"),
    //     ra = (int)RadioAddress,
    //     upml = UnitsPerMilliliter,
    //     med = (int)Medication,
    //     lot = (int)Lot,
    //     serial = (int)Serial,
    //     pro = (int)Progress,
    //     ps = NextPacketSequence,
    //     ms = NextMessageSequence,
    //     ri = NextRecordIndex,
    // });
    //     }
    // }

    public async Task LoadResponses()
    {
        Info.NextRecordIndex = 0;
        using (var conn = await _dataService.GetConnectionAsync())
        {
            var rs = await conn.QueryAsync("SELECT * FROM pod_message WHERE pod_id = @pod_id" +
                                           " ORDER BY record_index",
                new
                {
                    pod_id = Id.ToString("N")
                });

            foreach (var r in rs)
            {
                Info.NextRecordIndex = (int)r.record_index + 1;
                if (r.receive_data != null)
                {
                    var rd = (byte[]) r.receive_data;
                    var podMessage = PodMessage.FromBody(new Bytes(rd));
                    await ProcessResponseAsync(podMessage);
                }
            }
        }

        if (Info.Progress == PodProgress.Init0)
            Info.Progress = PodProgress.Running;
    }
    
    public async Task ProcessResponseAsync(IPodMessage message)
    {
        foreach (var part in message.Parts)
        {
            if (part is ResponseErrorPart ep)
                ProcessError(ep);
            if (part is ResponseStatusPart sp)
                ProcessStatus(sp);
            if (part is ResponseVersionPart rv)
                ProcessVersion(rv);
            if (part is ResponseInfoPart ri)
                ProcessInfo(ri);
        }
    }

    public uint NextNonce()
    {
        if (!Info.LastNonce.HasValue)
        {
            var b = new byte[4];
            new Random().NextBytes(b);
            Info.LastNonce = (uint)((b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3]);
        }
        else
        {
            Info.LastNonce = NonceTable[NonceIndex];
            NonceTable[NonceIndex] = GenerateNonce();
            NonceIndex = (int)((Info.LastNonce.Value & 0x0F) + 2);
        }

        return Info.LastNonce.Value;
    }

    public void SyncNonce(ushort syncWord, int syncMessageSequence)
    {
        uint w = (ushort)(Info.LastNonce.Value & 0xFFFF) + (uint)(CrcUtil.Crc16Table[syncMessageSequence] & 0xFFFF) + (uint)(Info.Lot & 0xFFFF) + (uint)(Info.Serial & 0xFFFF);
        var seed = (ushort)(((w & 0xFFFF) ^ syncWord) & 0xff);
        InitializeNonceTable(seed);
    }

    private void ProcessStatus(ResponseStatusPart part)
    {
        Info.Progress = part.Progress;
        Info.Faulted = part.Faulted;
        Info.ExtendedBolusActive = part.ExtendedBolusActive;
        Info.ImmediateBolusActive = part.ImmediateBolusActive;
        Info.TempBasalActive = part.TempBasalActive;
        Info.BasalActive = part.BasalActive;
        Info.PulsesDelivered = part.PulsesDelivered;
        Info.PulsesPending = part.PulsesPending;
        Info.PulsesRemaining = part.PulsesRemaining;
        Info.ActiveMinutes = part.ActiveMinutes;
        Info.UnackedAlertsMask = part.UnackedAlertsMask;
    }

    private void ProcessVersion(ResponseVersionPart part)
    {
        Info.Lot = part.Lot;
        Info.Serial = part.Serial;
        Info.Progress = part.Progress;
        if (part.PulseVolumeMicroUnits.HasValue)
            Info.PulseVolumeMicroUnits = part.PulseVolumeMicroUnits.Value;
        if (part.MaximumLifeTimeHours.HasValue)
            Info.MaximumLifeTimeHours = part.MaximumLifeTimeHours.Value;
    }

    private void ProcessError(ResponseErrorPart part)
    {
    }

    private void ProcessInfo(ResponseInfoPart part)
    {
        if (part is ResponseInfoActivationPart pact)
            ProcessInfoActivation(pact);
        if (part is ResponseInfoAlertsPart pale)
            ProcessInfoAlerts(pale);
        if (part is ResponseInfoExtendedPart pext)
            ProcessInfoExtended(pext);
        if (part is ResponseInfoPulseLogRecentPart plr)
            ProcessInfoPulseLogRecent(plr);
        if (part is ResponseInfoPulseLogLastPart pll)
            ProcessInfoPulseLogLast(pll);
        if (part is ResponseInfoPulseLogPreviousPart plp)
            ProcessInfoPulseLogPrevious(plp);
    }

    private void ProcessInfoExtended(ResponseInfoExtendedPart part)
    {
    }

    private void ProcessInfoAlerts(ResponseInfoAlertsPart part)
    {
    }

    private void ProcessInfoActivation(ResponseInfoActivationPart part)
    {
    }

    private void ProcessInfoPulseLogPrevious(ResponseInfoPulseLogPreviousPart part)
    {
    }

    private void ProcessInfoPulseLogLast(ResponseInfoPulseLogLastPart part)
    {
    }

    private void ProcessInfoPulseLogRecent(ResponseInfoPulseLogRecentPart part)
    {
    }

    private uint GenerateNonce()
    {
        NonceTable[0] = ((NonceTable[0] >> 16) + (NonceTable[0] & 0xFFFF) * 0x5D7F) & 0xFFFFFFFF;
        NonceTable[1] = ((NonceTable[1] >> 16) + (NonceTable[1] & 0xFFFF) * 0x8CA0) & 0xFFFFFFFF;
        return (NonceTable[1] + (NonceTable[0] << 16)) & 0xFFFFFFFF;
    }

    private void InitializeNonceTable(ushort seed)
    {
        NonceTable = new uint[18];
        NonceTable[0] = (uint)(((Info.Lot & 0xFFFF) + 0x55543DC3 + (Info.Lot >> 16) + (seed & 0xFF)) & 0xFFFFFFFF);
        NonceTable[1] = (uint)(((Info.Serial & 0xFFFF) + 0xAAAAE44E + (Info.Serial >> 16) + (seed >> 8)) & 0xFFFFFFFF);
        for (var i = 2; i < 18; i++) NonceTable[i] = GenerateNonce();

        NonceIndex = (int)(((NonceTable[0] + NonceTable[1]) & 0xF) + 2);
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}