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
        //foreach (var part in message.Parts)
        //{
        //    if (part is ResponseErrorPart ep)
        //        ProcessError(ep);
        //    if (part is ResponseStatusPart sp)
        //        ProcessStatus(sp, received);
        //    if (part is ResponseVersionPart rv)
        //        ProcessVersion(rv);
        //    if (part is ResponseInfoPart ri)
        //        ProcessInfo(ri, received);
        //}
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
}



