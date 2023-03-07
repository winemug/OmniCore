using System;
using System.Collections.Generic;
using OmniCore.Services.Interfaces.Definitions;

namespace OmniCore.Services.Interfaces.Entities;

public class PodRuntimeInformation
{
    public uint Lot { get; set; }
    public uint Serial { get; set; }
    public PodProgress Progress { get; set; }
    public int NextRecordIndex { get; set; }
    public int NextPacketSequence { get; set; }

    public int NextMessageSequence { get; set; }

    //
    public int PulseVolumeMicroUnits { get; set; }
    public int MaximumLifeTimeHours { get; set; }
    public uint? LastNonce { get; set; }
    public bool Faulted { get; set; }
    public bool ExtendedBolusActive { get; set; }
    public bool ImmediateBolusActive { get; set; }
    public bool TempBasalActive { get; set; }
    public bool BasalActive { get; set; }
    public int PulsesDelivered { get; set; }
    public int PulsesPending { get; set; }
    public int? PulsesRemaining { get; set; }
    public int ActiveMinutes { get; set; }
    public int UnackedAlertsMask { get; set; }

    public List<IMessagePart> ReceivedParts { get; set; }
    public DateTimeOffset? LastRadioPacketReceived { get; set; }
}