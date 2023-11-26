using OmniCore.Shared.Enums;
using OmniCore.Shared.Extensions;

namespace OmniCore.Shared.Entities.Omnipod.Parts;

public class ResponseVersionInfo : IMessagePart
{
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
    public required int RadioLowGain { get; set; }
    public required int Rssi { get; set; }
    public static IMessagePart ToInstance(Span<byte> span)
    {
         return new ResponseVersionInfo
         {
             HardwareVersionMajor = span[0],
             HardwareVersionMinor = span[1],
             HardwareVersionRevision = span[2],

             FirmwareVersionMajor = span[3],
             FirmwareVersionMinor = span[4],
             FirmwareVersionRevision = span[5],
             ProductId = span[6],
             Progress = (PodProgress)span[7],
             Lot = span[8..].Read32(),
             Serial = span[12..].Read32(),
             RadioLowGain = (int)span[16..].ReadBits(0, 2),
             Rssi = (int)span[16..].ReadBits(2, 6),
             AssignedAddress = span[17..].Read32()
         };
    }

    public int ToBytes(Span<byte> span)
    {
        span[0] = (byte)HardwareVersionMajor;
        span[1] = (byte)HardwareVersionMinor;
        span[2] = (byte)HardwareVersionRevision;
        span[3] = (byte)FirmwareVersionMajor;
        span[4] = (byte)FirmwareVersionMinor;
        span[5] = (byte)FirmwareVersionRevision;
        span[6] = (byte)ProductId;
        span[7..].WriteBits((int)Progress, 4, 4);
        span[8..].Write32(Lot);
        span[12..].Write32(Serial);
        span[16..].WriteBits(RadioLowGain, 0, 2);
        span[16..].WriteBits(Rssi, 2, 6);
        span[17..].Write32(AssignedAddress);
        return 21;
    }
}