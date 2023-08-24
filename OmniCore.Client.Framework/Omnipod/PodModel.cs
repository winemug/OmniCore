using System.Diagnostics;
using OmniCore.Client.Model;
using OmniCore.Common.Entities;
using OmniCore.Common.Pod;
using OmniCore.Framework.Omnipod.Responses;
using OmniCore.Shared.Enums;

namespace OmniCore.Framework.Omnipod;

public class PodModel : IPodModel
{
    private INonceProvider? _nonceProvider;
    private readonly Pod _pod;

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

    public INonceProvider? NonceProvider
    {
        get
        {
            if (_nonceProvider == null && VersionModel != null)
                _nonceProvider = new NonceProvider(VersionModel.Lot, VersionModel.Serial);
            return _nonceProvider;
        }
    }

    public DateTimeOffset? Activated { get; set; }
    public int NextRecordIndex { get; set; }
    public int NextPacketSequence { get; set; }
    public int NextMessageSequence { get; set; }
    public uint? LastNonce { get; set; }
    public DateTimeOffset? LastRadioPacketReceived { get; set; }

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

    public async Task LoadAsync()
    {
        NextRecordIndex = 0;
        using var ocdb = new OcdbContext();

        var pas = ocdb.PodActions
            .Where(pa => pa.PodId == _pod.PodId)
            .OrderBy(pa => pa.Index);

        foreach (var pa in pas)
        {
            NextRecordIndex = pa.Index + 1;
            ProcessAction(pa);
        }

        if (VersionModel == null && _pod.Lot.HasValue && _pod.Serial.HasValue)
        {
            VersionModel = new PodVersionModel
            {
                Lot = _pod.Lot.Value,
                Serial = _pod.Serial.Value,
                AssignedAddress = _pod.RadioAddress,
                FirmwareVersionMajor = 0,
                FirmwareVersionMinor = 0,
                FirmwareVersionRevision = 0,
                HardwareVersionMajor = 0,
                HardwareVersionMinor = 0,
                HardwareVersionRevision = 0,
                ProductId = 0
            };

            if (ActivationParametersModel == null) ActivationParametersModel = new PodActivationParametersModel();
        }
    }

    private void ProcessAction(PodAction pa)
    {
        // TODO: SentData and inconclusive affirmations for received & future
        if (pa.ReceivedData != null)
        {
            var received = pa.RequestSentLatest ?? pa.RequestSentEarliest;
            if (received.HasValue)
                try
                {
                    var receivedMessage = new MessageBuilder().Build(new Bytes(pa.ReceivedData));
                    NextMessageSequence = (receivedMessage.Sequence + 1) % 16;
                    ProcessReceivedMessage(receivedMessage, received.Value);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing received message\n{ex}");
                }
        }
    }

    private void ProcessMessage(StatusMessage md, DateTimeOffset received)
    {
        if (!md.ProgressModel.Faulted)
            Activated = received - TimeSpan.FromMinutes(md.StatusModel.ActiveMinutes);

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
        if (!md.ProgressModel.Faulted)
            Activated = received - TimeSpan.FromMinutes(md.StatusModel.ActiveMinutes);
        ProgressModel = md.ProgressModel;
        StatusModel = md.StatusModel;
        RadioMeasurementsModel = md.RadioMeasurementsModel;
        FaultInfoModel = md.FaultInfoModel;
    }
}