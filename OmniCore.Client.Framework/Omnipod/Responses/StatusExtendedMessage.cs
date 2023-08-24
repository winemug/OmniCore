using OmniCore.Common.Pod;

namespace OmniCore.Framework.Omnipod.Responses;

public class StatusExtendedMessage : IMessageData
{
    public PodProgressModel ProgressModel { get; set; }
    public PodStatusModel StatusModel { get; set; }
    public PodFaultInfoModel FaultInfoModel { get; set; }
    public PodRadioMeasurementsModel RadioMeasurementsModel { get; set; }

    public static Predicate<IMessageParts> CanParse =>
        parts =>
            parts.MainPart.Type == PodMessagePartType.ResponseInfo &&
            parts.MainPart.Data[0] == (byte)PodStatusType.Extended;

    public IMessageData FromParts(IMessageParts parts)
    {
        var data = parts.MainPart.Data;
        ProgressModel = new PodProgressModel
        {
            Progress = (PodProgress)data[1],
            Faulted = data[8] > 0
        };

        StatusModel = new PodStatusModel
        {
            BasalActive = (data[2] & 0x08) != 0,
            TempBasalActive = (data[2] & 0x04) != 0,
            ImmediateBolusActive = (data[2] & 0x02) != 0,
            ExtendedBolusActive = (data[2] & 0x01) != 0,
            PulsesPending = data.Word(3),
            LastProgrammingCommandSequence = data[5],
            PulsesDelivered = data.Word(6),
            PulsesRemaining = data.Word(11),
            ActiveMinutes = data.Word(13),
            UnackedAlertsMask = data[15]
        };

        FaultInfoModel = new PodFaultInfoModel
        {
            FaultEventCode = data[8],
            FaultEventMinute = data.Word(9),
            FaultPulseInformationInvalid = (data[16] & 0x02) != 0,
            FaultOcclusion = (data[16] & 0x01) != 0,
            FaultInsulinStateTable = (data[17] & 0x80) != 0,
            FaultOcclusionType = (data[17] & 0x60) >> 5,
            FaultDuringImmediateBolus = (data[17] & 0x10) != 0,
            ProgressBeforeFault = (PodProgress)(data[17] & 0xF)
            //ProgressBeforeFault2 = (PodProgress)data[19],
            //Unknown0 = data.Word(20),
        };

        RadioMeasurementsModel = new PodRadioMeasurementsModel
        {
            RadioLowGain = (data[18] & 0xC0) >> 6,
            Rssi = data[18] & 0b00111111
        };

        return this;
    }

    public IMessageParts ToParts()
    {
        throw new NotImplementedException();
    }
}