namespace OmniCore.Common.Pod;

public class PodVersionModel
{
    public int HardwareVersionMajor { get; init; }
    public int HardwareVersionMinor { get; init; }
    public int HardwareVersionRevision { get; init; }
    public int FirmwareVersionMajor { get; init; }
    public int FirmwareVersionMinor { get; init; }
    public int FirmwareVersionRevision { get; init; }
    public int ProductId { get; init; }
    public uint Lot { get; init; }
    public uint Serial { get; init; }
    public uint AssignedAddress { get; init; }
}