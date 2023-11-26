using OmniCore.Shared.Enums;
using OmniCore.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Shared.Entities.Omnipod.Parts;

public class ResponseInfoExtendedStatus : IMessagePart
{
    public required bool ExtendedBolusActive { get; init; }
    public required bool ImmediateBolusActive { get; init; }
    public required bool TempBasalActive { get; init; }
    public required bool BasalActive { get; init; }
    public required int PulsesDelivered { get; init; }
    public required int PulsesPending { get; init; }
    public required int PulsesRemaining { get; init; }
    public required int ActiveMinutes { get; init; }
    public required int UnackedAlertsMask { get; set; }
    public required int LastProgrammingCommandSequence { get; set; }
    public required int FaultEvent { get; set; }
    public required int FaultEventMinute { get; set; }
    public required bool PulseInformationInvalid { get; set; }
    public required bool OcclusionFault { get; set; }
    public required PodProgress Progress { get; set; }
    public required bool StateTableCorruption { get; set; }
    public required bool FaultWhileImmediateBolus { get; set; }
    public required int OcclusionType { get; set; }
    public required PodProgress ProgressBeforeFault { get; set; }
    public required int RadioLowGain { get; set; }
    public required int Rssi { get; set; }

    public static IMessagePart ToInstance(Span<byte> span)
    {
        return new ResponseInfoExtendedStatus
        {
            Progress = (PodProgress)span[1..2].ReadBits(4, 4),

            ExtendedBolusActive = span[2..3].ReadBit(4),
            ImmediateBolusActive = span[2..3].ReadBit(5),
            TempBasalActive = span[2..3].ReadBit(6),
            BasalActive = span[2..3].ReadBit(7),

            PulsesPending = span[3..5].Read16(),
            LastProgrammingCommandSequence = span[5],
            PulsesDelivered = span[6..8].Read16(),
            FaultEvent = span[8],
            FaultEventMinute = span[9..11].Read16(),
            PulsesRemaining = span[11..13].Read16(),
            ActiveMinutes = span[13..15].Read16(),
            UnackedAlertsMask = span[15],
            PulseInformationInvalid = span[16..].ReadBit(6),
            OcclusionFault = span[16..].ReadBit(7),
            StateTableCorruption = span[17..].ReadBit(0),
            OcclusionType = (int)span[17..].ReadBits(1, 2),
            FaultWhileImmediateBolus = span[17..].ReadBit(3),
            ProgressBeforeFault = (PodProgress)span[17..].ReadBits(4, 4),
            RadioLowGain = (int)span[17..].ReadBits(0, 2),
            Rssi = (int)span[18..].ReadBits(2, 6),
        };
    }

    public int ToBytes(Span<byte> span)
    {
        return 0;
    }

}
