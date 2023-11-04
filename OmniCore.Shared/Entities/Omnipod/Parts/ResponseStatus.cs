using OmniCore.Shared.Entities;
using OmniCore.Shared.Entities.Omnipod.Parts;
using OmniCore.Shared.Enums;
using OmniCore.Shared.Extensions;

namespace OmniCore.Framework.Omnipod.Parts;

public class ResponseStatus : IMessagePart
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
    public required bool OcclusionFault { get; set; }
    public required PodProgress Progress { get; set; }
    public static IMessagePart ToInstance(Span<byte> span)
    {
        return new ResponseStatus
        {
            ExtendedBolusActive = span.ReadBit(0),
            ImmediateBolusActive = span.ReadBit(1),
            TempBasalActive = span.ReadBit(2),
            BasalActive = span.ReadBit(3),
            Progress = (PodProgress)span.ReadBits(4, 4),
            PulsesDelivered = (int)span.ReadBits(12, 13),
            LastProgrammingCommandSequence = (int)span.ReadBits(25, 4),
            PulsesPending = (int)span.ReadBits(29, 11),
            OcclusionFault = span.ReadBit(40),
            UnackedAlertsMask = (int) span.ReadBits(41, 8),
            ActiveMinutes = (int) span.ReadBits(49, 13),
            PulsesRemaining = (int) span.ReadBits(62, 10)
        };
    }

    public int ToBytes(Span<byte> span)
    {
        span.WriteBit(ExtendedBolusActive, 0);
        span.WriteBit(ImmediateBolusActive, 1);
        span.WriteBit(TempBasalActive, 2);
        span.WriteBit(BasalActive, 3);
        span.WriteBits((uint)Progress, 4, 4);
        span.WriteBits(PulsesDelivered, 12, 13);
        span.WriteBits(LastProgrammingCommandSequence, 25, 4);
        span.WriteBits(PulsesPending, 29, 11);
        span.WriteBit(OcclusionFault, 40);
        span.WriteBits(UnackedAlertsMask, 41, 8);
        span.WriteBits(ActiveMinutes, 49, 13);
        span.WriteBits(PulsesRemaining, 62, 10);
        return 9;
    }
}