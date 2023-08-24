using OmniCore.Common.Pod;

namespace OmniCore.Framework.Omnipod.Responses;

public class VersionExtendedMessage : IMessageData
{
    public PodVersionModel VersionModel { get; set; }
    public PodActivationParametersModel ActivationParametersModel { get; set; }
    public PodProgressModel ProgressModel { get; set; }

    public static Predicate<IMessageParts> CanParse =>
        parts =>
            parts.MainPart.Type == PodMessagePartType.ResponseVersionInfo &&
            parts.MainPart.Data.Length == 27;

    public IMessageData FromParts(IMessageParts parts)
    {
        var data = parts.MainPart.Data;
        ActivationParametersModel = new PodActivationParametersModel
        {
            PulseVolumeMicroUnits = data.Word(0),
            PulseRatePer125ms = data[2],
            PrimingPulseRatePer125ms = data[3],
            PrimingPulseCount = data[4],
            CannulaInsertPulseCount = data[5],
            MaximumLifeTimeHours = data[6]
        };

        VersionModel = new PodVersionModel
        {
            HardwareVersionMajor = data[7],
            HardwareVersionMinor = data[8],
            HardwareVersionRevision = data[9],

            FirmwareVersionMajor = data[10],
            FirmwareVersionMinor = data[11],
            FirmwareVersionRevision = data[12],

            ProductId = data[13],
            Lot = data.DWord(15),
            Serial = data.DWord(19),
            AssignedAddress = data.DWord(23)
        };
        ProgressModel = new PodProgressModel
        {
            Progress = (PodProgress)data[14],
            Faulted = data[14] > 9 && data[14] < 15
        };
        return this;
    }

    public IMessageParts ToParts()
    {
        throw new NotImplementedException();
    }
}