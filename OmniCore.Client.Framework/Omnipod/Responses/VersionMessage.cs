using OmniCore.Common.Pod;

namespace OmniCore.Framework.Omnipod.Responses;

public class VersionMessage : IMessageData
{
    public PodVersionModel VersionModel { get; set; }
    public PodRadioMeasurementsModel RadioMeasurementsModel { get; set; }
    public PodProgressModel ProgressModel { get; set; }

    public static Predicate<IMessageParts> CanParse =>
        parts =>
            parts.MainPart.Type == PodMessagePartType.ResponseVersionInfo &&
            parts.MainPart.Data.Length == 21;

    public IMessageData FromParts(IMessageParts parts)
    {
        var data = parts.MainPart.Data;
        VersionModel = new PodVersionModel
        {
            HardwareVersionMajor = data[0],
            HardwareVersionMinor = data[1],
            HardwareVersionRevision = data[2],

            FirmwareVersionMajor = data[3],
            FirmwareVersionMinor = data[4],
            FirmwareVersionRevision = data[5],
            ProductId = data[6],
            Lot = data.DWord(8),
            Serial = data.DWord(12),
            AssignedAddress = data.DWord(17)
        };

        RadioMeasurementsModel = new PodRadioMeasurementsModel
        {
            RadioLowGain = (data[16] >> 6) & 0b00000011,
            Rssi = data[16] & 0b00111111
        };
        ProgressModel = new PodProgressModel
        {
            Progress = (PodProgress)data[7],
            Faulted = data[14] > 9 && data[7] < 15
        };
        return this;
    }

    public IMessageParts ToParts()
    {
        throw new NotImplementedException();
    }
}