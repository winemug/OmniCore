using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Nito.AsyncEx;
using OmniCore.Common.Data;
using OmniCore.Common.Pod;
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
        _pod = pod;
        InitializeNonceTable(0);
    }

    public Guid Id => _pod.PodId;
    public uint RadioAddress => _pod.RadioAddress;
    public int UnitsPerMilliliter => _pod.UnitsPerMilliliter;
    public MedicationType Medication => _pod.Medication;
   
    // Runtime Info
    public PodProgressModel? ProgressModel { get; set; }
    public PodStatusModel? StatusModel { get; set; }
    public PodFaultInfoModel? FaultInfoModel { get; set; }
    public PodVersionModel? VersionModel { get; set; }
    public PodRadioMeasurementsModel? RadioMeasurementsModel { get; set; }
    public PodActivationParametersModel? ActivationParametersModel { get; set; }
    public PodBasalModel? BasalModel { get; set; }

    public DateTimeOffset? StatusUpdated { get; set; }
    public int NextRecordIndex { get; set; }
    public int NextPacketSequence { get; set; }
    public int NextMessageSequence { get; set; }
    public uint? LastNonce { get; set; }
    public DateTimeOffset? LastRadioPacketReceived { get; set; }

    public async Task Load()
    {
        NextRecordIndex = 0;
        using var ocdb = new OcdbContext();

        var pas = ocdb.PodActions.Where(pa => pa.PodId == _pod.PodId)
            .OrderBy(pa => pa.Index);

        foreach (var pa in pas)
        {
            NextRecordIndex = pa.Index + 1;
            await ProcessActionAsync(pa);
        }

        if (_pod.ImportedProperties != null)
        {
            if (VersionModel == null)
            {
                VersionModel = new PodVersionModel
                {
                    Lot = _pod.ImportedProperties.Lot,
                    Serial = _pod.ImportedProperties.Serial,
                    AssignedAddress = _pod.RadioAddress,
                    FirmwareVersionMajor = 0,
                    FirmwareVersionMinor = 0,
                    FirmwareVersionRevision = 0,
                    HardwareVersionMajor = 0,
                    HardwareVersionMinor = 0,
                    HardwareVersionRevision = 0,
                    ProductId = 0
                };
            }

            if (ActivationParametersModel == null)
            {
                ActivationParametersModel = new PodActivationParametersModel
                {
                };
            }

            if (BasalModel == null)
            {
                var storedRates = _pod.ImportedProperties.ActiveBasalRates;
                int[] basalRates;
                if (storedRates.Length == 1)
                {
                    basalRates = new int[48];
                    for (int i = 0; i < 48; i++)
                        basalRates[i] = storedRates[0];
                }
                else
                {
                    basalRates = storedRates;
                }

                BasalModel = new PodBasalModel
                {
                    BasalSchedule = basalRates,
                    PodTimeReferenceValue = _pod.ImportedProperties.PodTimeReferenceValue,
                    PodTimeReference = _pod.ImportedProperties.PodTimeReference,
                };
            }
        }
    }

    public async Task ProcessActionAsync(PodAction pa)
    {
        // TODO: SentData and inconclusive affirmations for received & future
        if (pa.ReceivedData != null)
        {
            var received = pa.RequestSentLatest ?? pa.RequestSentEarliest;
            if (received.HasValue)
            {
                var receivedMessage = PodMessage.FromBody(new Bytes(pa.ReceivedData));
                if (receivedMessage != null)
                    await ProcessReceivedMessageAsync(receivedMessage, received.Value);
            }
        }
    }
    
    public async Task ProcessReceivedMessageAsync(IPodMessage message, DateTimeOffset received)
    {
        foreach (var part in message.Parts)
        {
            if (part is ResponseErrorPart ep)
                ProcessError(ep);
            if (part is ResponseStatusPart sp)
                ProcessStatus(sp, received);
            if (part is ResponseVersionPart rv)
                ProcessVersion(rv);
            if (part is ResponseInfoPart ri)
                ProcessInfo(ri, received);
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

    private void ProcessStatus(ResponseStatusPart part, DateTimeOffset received)
    {
        StatusUpdated = received;
        ProgressModel = part.ProgressModel;
        StatusModel = part.StatusModel;
    }

    private void ProcessVersion(ResponseVersionPart part)
    {
        // Lot = part.Lot;
        // Serial = part.Serial;
        // Progress = part.Progress;
        // if (part.PulseVolumeMicroUnits.HasValue)
        //     PulseVolumeMicroUnits = part.PulseVolumeMicroUnits.Value;
        // if (part.MaximumLifeTimeHours.HasValue)
        //     MaximumLifeTimeHours = part.MaximumLifeTimeHours.Value;
    }

    private void ProcessError(ResponseErrorPart part)
    {
    }

    private void ProcessInfo(ResponseInfoPart part, DateTimeOffset received)
    {
        if (part is ResponseInfoActivationPart pact)
            ProcessInfoActivation(pact);
        if (part is ResponseInfoAlertsPart pale)
            ProcessInfoAlerts(pale);
        if (part is ResponseInfoExtendedPart pext)
            ProcessInfoExtended(pext, received);
        if (part is ResponseInfoPulseLogRecentPart plr)
            ProcessInfoPulseLogRecent(plr);
        if (part is ResponseInfoPulseLogLastPart pll)
            ProcessInfoPulseLogLast(pll);
        if (part is ResponseInfoPulseLogPreviousPart plp)
            ProcessInfoPulseLogPrevious(plp);
    }

    private void ProcessInfoExtended(ResponseInfoExtendedPart part, DateTimeOffset received)
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



