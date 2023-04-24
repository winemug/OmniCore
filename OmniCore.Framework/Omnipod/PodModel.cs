using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Nito.AsyncEx;
using OmniCore.Common.Data;
using OmniCore.Common.Pod;
using OmniCore.Framework.Omnipod;
using OmniCore.Framework.Omnipod.Responses;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class PodModel : IPodModel
{
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
    public NonceProvider? NonceProvider { get; set; }

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
                try
                {
                    var receivedMessage = new MessageBuilder().Build(new Bytes(pa.ReceivedData));
                    ProcessReceivedMessage(receivedMessage, received.Value);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing received message\n{ex}");
                }
            }
        }
    }
    
    public void ProcessReceivedMessage(IPodMessage message, DateTimeOffset received)
    {
        var messageData = message.Data;
        if (messageData is StatusMessage sm)
            ProcessMessage(sm, received);
        if (messageData is VersionMessage vm)
            ProcessMessage(vm, received);
        if (messageData is VersionExtendedMessage vem)
            ProcessMessage(vem, received);
        if (messageData is StatusExtendedMessage sem)
            ProcessMessage(sem, received);
    }

    private void ProcessMessage(StatusMessage md, DateTimeOffset received)
    {
        StatusUpdated = received;
        ProgressModel = md.ProgressModel;
        StatusModel = md.StatusModel;
    }

    private void ProcessMessage(VersionMessage md, DateTimeOffset received)
    {
        VersionModel = md.VersionModel;
        ProgressModel = md.ProgressModel;
        RadioMeasurementsModel = md.RadioMeasurementsModel;
    }

    private void ProcessMessage(VersionExtendedMessage md, DateTimeOffset received)
    {
        VersionModel = md.VersionModel;
        ProgressModel = md.ProgressModel;
        ActivationParametersModel = md.ActivationParametersModel;
    }

    private void ProcessMessage(StatusExtendedMessage md, DateTimeOffset received)
    {
        StatusUpdated = received;
        ProgressModel = md.ProgressModel;
        StatusModel = md.StatusModel;
        RadioMeasurementsModel = md.RadioMeasurementsModel;
        FaultInfoModel = md.FaultInfoModel;
    }
}
