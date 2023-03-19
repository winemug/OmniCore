using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Services.Interfaces.Pod;

public interface IPod
{
    Guid Id { get; set; }
    uint RadioAddress { get; set; }
    int UnitsPerMilliliter { get; set; }
    MedicationType Medication { get; set; }
    DateTimeOffset ValidFrom { get; set; }
    DateTimeOffset? ValidTo { get; set; }
    uint Lot { get; set; }
    uint Serial { get; set; }
    PodProgress Progress { get; set; }
    int NextRecordIndex { get; set; }
    int NextPacketSequence { get; set; }
    int NextMessageSequence { get; set; }
    int PulseVolumeMicroUnits { get; set; }
    int MaximumLifeTimeHours { get; set; }
    uint? LastNonce { get; set; }
    bool Faulted { get; set; }
    bool ExtendedBolusActive { get; set; }
    bool ImmediateBolusActive { get; set; }
    bool TempBasalActive { get; set; }
    bool BasalActive { get; set; }
    int PulsesDelivered { get; set; }
    int PulsesPending { get; set; }
    int? PulsesRemaining { get; set; }
    int ActiveMinutes { get; set; }
    int UnackedAlertsMask { get; set; }

    List<IMessagePart> ReceivedParts { get; set; }
    DateTimeOffset? LastRadioPacketReceived { get; set; }
    
    Task<IDisposable> LockAsync(CancellationToken cancellationToken);
    Task ProcessResponseAsync(IPodMessage message);
    uint NextNonce();
    void SyncNonce(ushort syncWord, int syncMessageSequence);
    Task LoadResponses();
}