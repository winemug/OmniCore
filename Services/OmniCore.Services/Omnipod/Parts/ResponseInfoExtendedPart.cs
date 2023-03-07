using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class ResponseInfoExtendedPart : ResponseInfoPart
{
    public ResponseInfoExtendedPart(Bytes data)
    {
        Progress = (PodProgress)data[1];
        BasalActive = (data[2] & 0x08) != 0;
        TempBasalActive = (data[2] & 0x04) != 0;
        ImmediateBolusActive = (data[2] & 0x02) != 0;
        ExtendedBolusActive = (data[2] & 0x01) != 0;
        PulsesPending = data.Word(3);
        LastProgrammingCommandSequence = data[5];
        PulsesDelivered = data.Word(6);
        FaultEventCode = data[8];
        FaultEventMinute = data.Word(9);
        var pr = data.Word(11);
        PulsesRemaining = pr < 0x3ff ? pr : null;
        ActiveMinutes = data.Word(13);
        UnackedAlertsMask = data[15];
        FaultPulseInformationInvalid = (data[16] & 0x02) != 0;
        FaultOcclusion = (data[16] & 0x01) != 0;
        FaultInsulinStateTable = (data[17] & 0x80) != 0;
        FaultOcclusionType = (data[17] & 0x60) >> 5;
        FaultDuringImmediateBolus = (data[17] & 0x10) != 0;
        ProgressBeforeFault = (PodProgress)(data[17] & 0xF);
        RadioGain = (data[18] & 0xC0) >> 6;
        Rssi = data[18] & 0b00111111;
        ProgressBeforeFault2 = (PodProgress)data[19];
        Unknown0 = data.Word(20);
        Data = data;
    }

    public override RequestStatusType StatusType => RequestStatusType.Extended;

    public PodProgress Progress { get; }
    public bool ExtendedBolusActive { get; }
    public bool ImmediateBolusActive { get; }
    public bool TempBasalActive { get; }
    public bool BasalActive { get; }
    public int PulsesDelivered { get; }
    public int PulsesPending { get; }
    public int FaultEventCode { get; }
    public int FaultEventMinute { get; }

    public bool FaultPulseInformationInvalid { get; }
    public bool FaultOcclusion { get; }
    public bool FaultInsulinStateTable { get; }
    public int FaultOcclusionType { get; }
    public bool FaultDuringImmediateBolus { get; }
    public PodProgress ProgressBeforeFault { get; }
    public PodProgress ProgressBeforeFault2 { get; }
    public int RadioGain { get; }
    public int Rssi { get; }
    public int? PulsesRemaining { get; }
    public int ActiveMinutes { get; }
    public int UnackedAlertsMask { get; }

    public ushort Unknown0 { get; }
    public int LastProgrammingCommandSequence { get; }
}