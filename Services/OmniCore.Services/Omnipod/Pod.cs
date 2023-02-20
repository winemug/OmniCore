using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Nito.AsyncEx;
using Xamarin.Forms;

namespace OmniCore.Services;

public class Pod
{
    public Guid Id { get; set; }
    public uint RadioAddress { get; set; }
    public int UnitsPerMilliliter { get; set; }
    public MedicationType Medication { get; set; }
    public uint Lot { get; set; }
    public uint Serial { get; set; }
    public PodProgress Progress { get; set; }
    public int NextPacketSequence { get; set; }
    public int NextMessageSequence { get; set; }
    public DateTimeOffset Entered { get; set; }
    public DateTimeOffset? Removed { get; set; }

    //
    public int PulseVolumeMicroUnits { get; set; }
    public int MaximumLifeTimeHours { get; set; }
    public uint? LastNonce { get; private set; }
    public bool Faulted { get; set; }
    public bool ExtendedBolusActive { get; set; }
    public bool ImmediateBolusActive { get; set;}
    public bool TempBasalActive { get; set;}
    public bool BasalActive { get; set;}
    public int PulsesDelivered { get; set;}
    public int PulsesPending { get; set;}
    public int? PulsesRemaining { get; set;}
    public int ActiveMinutes { get; set;}
    public int UnackedAlertsMask { get; set;}

    public List<MessagePart> ReceivedParts = new List<MessagePart>();
    public DateTimeOffset? LastRadioPacketReceived { get; set; }
    
    private AsyncLock _allocationLock = new ();
    private DataStore _dataStore;

    public Pod(DataStore dataStore)
    {
        _dataStore = dataStore;
        Id = Guid.NewGuid();
        var r = new Random();
        var bn0 = r.Next(13);
        var bn1 = r.Next(16);
        var b0 = (bn0 + 2) << 4 | bn1;
        var b123 = new byte[3];
        r.NextBytes(b123);
        RadioAddress = (uint)(b0 << 24 | b123[0] << 16 | b123[1] << 8 | b123[2]);
        
        InitializeNonceTable(0);
    }

    public async Task<IDisposable> LockAsync(CancellationToken cancellationToken)
    {
        return await _allocationLock.LockAsync(cancellationToken);
    }

    public async Task Save()
    {
        using (var conn = await _dataStore.GetConnectionAsync())
        {
            await conn.ExecuteAsync(
    "UPDATE pod SET radio_address=@ra, units_per_ml=@upml, medication=@med, lot=@lot, serial=@serial, progress=@pro," +
    " packet_sequence=@ps, message_sequence=@ms WHERE id = @id",
    new
    {
        id = Id.ToString("N"),
        ra = (int)RadioAddress,
        upml = UnitsPerMilliliter,
        med = (int)Medication,
        lot = (int)Lot,
        serial = (int)Serial,
        pro = (int)Progress,
        ps = NextPacketSequence,
        ms = NextMessageSequence,
    });
        }
    }

    public async Task ProcessResponseAsync(PodMessage message)
    {
        foreach (var part in message.Parts)
        {
            ReceivedParts.Add(part);
            
            if (part is ResponseErrorPart ep)
                ProcessError(ep);
            if (part is ResponseStatusPart sp)
                ProcessStatus(sp);
            if (part is ResponseVersionPart rv)
                ProcessVersion(rv);
            if (part is ResponseInfoPart ri)
                ProcessInfo(ri);
        }
        await Save();
    }

    private void ProcessStatus(ResponseStatusPart part)
    {
        Progress = part.Progress;
        Faulted = part.Faulted;
        ExtendedBolusActive = part.ExtendedBolusActive;
        ImmediateBolusActive = part.ImmediateBolusActive;
        TempBasalActive = part.TempBasalActive;
        BasalActive = part.BasalActive;
        PulsesDelivered = part.PulsesDelivered;
        PulsesPending = part.PulsesPending;
        PulsesRemaining = part.PulsesRemaining;
        ActiveMinutes = part.ActiveMinutes;
        UnackedAlertsMask = part.UnackedAlertsMask;
    }
    
    private void ProcessVersion(ResponseVersionPart part)
    {
        Lot = part.Lot;
        Serial = part.Serial;
        Progress = part.Progress;
        if (part.PulseVolumeMicroUnits.HasValue)
            PulseVolumeMicroUnits = part.PulseVolumeMicroUnits.Value;
        if (part.MaximumLifeTimeHours.HasValue)
            MaximumLifeTimeHours = part.MaximumLifeTimeHours.Value;
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

    public uint NextNonce()
    {
        if (!LastNonce.HasValue)
        {
            var b = new byte[4];
            new Random().NextBytes(b);
            LastNonce = (uint)(b[0] << 24 | b[1] << 16 | b[2] << 8 | b[3]);
        }
        else
        {
            LastNonce = NonceTable[NonceIndex];
            NonceTable[NonceIndex] = GenerateNonce();
            NonceIndex = (int)((LastNonce.Value & 0x0F) + 2);
        }

        return LastNonce.Value;
    }

    public void SyncNonce(ushort syncWord, int syncMessageSequence)
    {
        var w = (LastNonce.Value & 0xFFFF) + (CrcUtil.Crc16Table[syncMessageSequence] & 0xFFFF) + (Lot & 0xFFFF) +
                (Serial & 0xFFFF);
        var seed = (ushort)(((w & 0xFFFF) ^ syncWord) & 0xff);
        InitializeNonceTable(seed);
    }

    private uint[] NonceTable;
    private int NonceIndex = 0;

    private uint GenerateNonce()
    {
        NonceTable[0] = ((NonceTable[0] >> 16) + (NonceTable[0] & 0xFFFF) * 0x5D7F) & 0xFFFFFFFF;
        NonceTable[1] = ((NonceTable[1] >> 16) + (NonceTable[1] & 0xFFFF) * 0x8CA0) & 0xFFFFFFFF;
        return (NonceTable[1] + (NonceTable[0] << 16)) & 0xFFFFFFFF;
    }
    
    private void InitializeNonceTable(ushort seed)
    {
        NonceTable = new uint[18];
        NonceTable[0] = (uint)(((Lot & 0xFFFF) + 0x55543DC3 + (Lot >> 16) + (seed & 0xFF)) & 0xFFFFFFFF);
        NonceTable[1] = (uint)(((Serial & 0xFFFF) + 0xAAAAE44E + (Serial >> 16) + (seed >> 8)) & 0xFFFFFFFF);
        for (int i = 2; i < 18; i++)
        {
            NonceTable[i] = GenerateNonce();
        }

        NonceIndex = (int)(((NonceTable[0] + NonceTable[1]) & 0xF) + 2);
    }
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}