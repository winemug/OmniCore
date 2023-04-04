using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Services.Interfaces.Pod;

public interface IPodModel
{
    Guid Id { get; }
    uint RadioAddress { get; }
    int UnitsPerMilliliter { get; }
    MedicationType Medication { get; }
    uint? Lot { get; }
    uint? Serial { get; }
    PodProgress? Progress { get; set; }
    int NextRecordIndex { get; set; }
    int NextPacketSequence { get; set; }
    int NextMessageSequence { get; set; }
    int? PulseVolumeMicroUnits { get; set; }
    int? MaximumLifeTimeHours { get; set; }
    uint? LastNonce { get; set; }
    bool? Faulted { get; set; }
    bool? ExtendedBolusActive { get; set; }
    bool? ImmediateBolusActive { get; set; }
    bool? TempBasalActive { get; set; }
    bool? BasalActive { get; set; }
    int? PulsesDelivered { get; set; }
    int? PulsesPending { get; set; }
    int? PulsesRemaining { get; set; }
    int? ActiveMinutes { get; set; }
    int? UnackedAlertsMask { get; set; }
    DateTimeOffset? LastUpdated { get; set; }
    DateTimeOffset? LastRadioPacketReceived { get; set; }
    Task ProcessResponseAsync(IPodMessage message);
    uint NextNonce();
    void SyncNonce(ushort syncWord, int syncMessageSequence);
}