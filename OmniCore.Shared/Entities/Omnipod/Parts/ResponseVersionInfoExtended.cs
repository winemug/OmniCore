using OmniCore.Shared.Enums;
using OmniCore.Shared.Extensions;

namespace OmniCore.Shared.Entities.Omnipod.Parts;

public class ResponseVersionInfoExtended : IMessagePart
{
    public required int PulseVolumeMicroUnits { get; init; }
    public required int PulseRatePer125Ms { get; init; }
    public required int PrimingPulseRatePer125Ms { get; init; }
    public required int PrimingPulseCount { get; init; }
    public required int CannulaInsertPulseCount { get; init; }
    public required int MaximumLifeTimeHours { get; init; }

    public required int HardwareVersionMajor { get; init; }
    public required int HardwareVersionMinor { get; init; }
    public required int HardwareVersionRevision { get; init; }
    public required int FirmwareVersionMajor { get; init; }
    public required int FirmwareVersionMinor { get; init; }
    public required int FirmwareVersionRevision { get; init; }
    public required int ProductId { get; init; }
    public required uint Lot { get; init; }
    public required uint Serial { get; init; }
    public required uint AssignedAddress { get; init; }
    public required PodProgress Progress { get; set; }

    public static IMessagePart ToInstance(Span<byte> span)
    {
        return new ResponseVersionInfoExtended
        {
            PulseVolumeMicroUnits = span.Read16(),
            PulseRatePer125Ms = span[2],
            PrimingPulseRatePer125Ms = span[3],
            PrimingPulseCount = span[4],
            CannulaInsertPulseCount = span[5],
            MaximumLifeTimeHours = span[6],
            HardwareVersionMajor = span[7],
            HardwareVersionMinor = span[8],
            HardwareVersionRevision = span[9],
            FirmwareVersionMajor = span[10],
            FirmwareVersionMinor = span[11],
            FirmwareVersionRevision = span[12],
            ProductId = span[13],
            Progress = (PodProgress)span[14],
            Lot = span[15..].Read32(),
            Serial = span[19..].Read32(),
            AssignedAddress = span[23..].Read32()
        };
    }

    public int ToBytes(Span<byte> span)
    {
        throw new NotImplementedException();
    }
}