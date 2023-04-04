using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Nito.AsyncEx;
using OmniCore.Common.Data;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class PodModel : IPodModel
{
    private int NonceIndex;
    private uint[] NonceTable;
    private Pod _pod;

    public PodModel(Pod pod)
    {
        InitializeNonceTable(0);
        RadioAddress = pod.RadioAddress;
        UnitsPerMilliliter = pod.UnitsPerMilliliter;
        Medication = pod.Medication;
    }

    public Guid Id => _pod.PodId;
    public uint RadioAddress { get; }
    public int UnitsPerMilliliter { get; }
    public MedicationType Medication { get; }
    // Runtime Info
    public uint? Lot { get; set; }
    public uint? Serial { get; set; }
    public PodProgress? Progress { get; set; }
    public int NextRecordIndex { get; set; }
    public int NextPacketSequence { get; set; }
    public int NextMessageSequence { get; set; }
    public int? PulseVolumeMicroUnits { get; set; }
    public int? MaximumLifeTimeHours { get; set; }
    public uint? LastNonce { get; set; }
    public bool? Faulted { get; set; }
    public bool? ExtendedBolusActive { get; set; }
    public bool? ImmediateBolusActive { get; set; }
    public bool? TempBasalActive { get; set; }
    public bool? BasalActive { get; set; }
    public int? PulsesDelivered { get; set; }
    public int? PulsesPending { get; set; }
    public int? PulsesRemaining { get; set; }
    public int? ActiveMinutes { get; set; }
    public int? UnackedAlertsMask { get; set; }
    
    public DateTimeOffset? LastUpdated { get; set; }
    public DateTimeOffset? LastRadioPacketReceived { get; set; }

    public async Task LoadResponses()
    {
        NextRecordIndex = 0;
        using var ocdb = new OcdbContext();
        await ocdb.Entry(_pod).Collection(p => p.Actions).LoadAsync();

        foreach (var pa in _pod.Actions.OrderBy(pa => pa.Index))
        {
            NextRecordIndex = pa.Index + 1;
            if (pa.Received != null)
            {
                var podMessage = PodMessage.FromBody(new Bytes(pa.Received));
                if (podMessage != null)
                    await ProcessResponseAsync(podMessage);
            }
        }

        if (_pod.IsImported)
        {
            Lot ??= _pod.Lot;
            Serial ??= _pod.Serial;
            Progress ??= PodProgress.Running;
        }
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
        if (!LastNonce.HasValue)
        {
            var b = new byte[4];
            new Random().NextBytes(b);
            LastNonce = (uint)((b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3]);
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
        uint w = (ushort)(LastNonce.Value & 0xFFFF) + (uint)(CrcUtil.Crc16Table[syncMessageSequence] & 0xFFFF) + (uint)(Lot & 0xFFFF) + (uint)(Serial & 0xFFFF);
        var seed = (ushort)(((w & 0xFFFF) ^ syncWord) & 0xff);
        InitializeNonceTable(seed);
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
        for (var i = 2; i < 18; i++) NonceTable[i] = GenerateNonce();

        NonceIndex = (int)(((NonceTable[0] + NonceTable[1]) & 0xF) + 2);
    }
}