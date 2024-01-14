using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Common.Data;
using OmniCore.Common.Pod;

namespace OmniCore.Services.Interfaces.Pod;

public interface IPodModel
{
    Guid Id { get; }
    DateTimeOffset Created { get; }
    uint RadioAddress { get; }
    int UnitsPerMilliliter { get; }
    MedicationType Medication { get; }

    // Runtime Info
    PodProgressModel? ProgressModel { get; set; }
    PodStatusModel? StatusModel { get; set; }
    PodFaultInfoModel? FaultInfoModel { get; set; }
    PodVersionModel? VersionModel { get; set; }
    PodRadioMeasurementsModel? RadioMeasurementsModel { get; set; }
    PodActivationParametersModel? ActivationParametersModel { get; set; }
    PodBasalModel? BasalModel { get; set; }
    INonceProvider? NonceProvider { get; }

    DateTimeOffset? Activated { get; set; }
    int NextRecordIndex { get; set; }
    int NextPacketSequence { get; set; }
    int NextMessageSequence { get; set; }
    uint? LastNonce { get; set; }
    DateTimeOffset? LastRadioPacketReceived { get; set; }

    void ProcessReceivedMessage(IPodMessage message, DateTimeOffset received);
}