using OmniCore.Common.Pod;
using OmniCore.Services.Interfaces.Pod;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OmniCore.Framework.Omnipod.Responses;

public class StatusExtendedMessage : IMessageData
{
    public static Predicate<IMessageParts> CanParse =>
        (parts) =>
            parts.MainPart.Type == PodMessagePartType.ResponseInfo &&
            parts.MainPart.Data[0] == (byte)PodStatusType.Extended;

    public PodProgress Progress { get; set;}
    public bool ExtendedBolusActive { get; set;}
    public bool ImmediateBolusActive { get; set;}
    public bool TempBasalActive { get; set;}
    public bool BasalActive { get; set;}
    public int PulsesDelivered { get; set;}
    public int PulsesPending { get; set;}
    public int FaultEventCode { get; set;}
    public int FaultEventMinute { get; set;}
    public bool FaultPulseInformationInvalid { get; set;}
    public bool FaultOcclusion { get; set;}
    public bool FaultInsulinStateTable { get; set;}
    public int FaultOcclusionType { get; set;}
    public bool FaultDuringImmediateBolus { get; set;}
    public PodProgress ProgressBeforeFault { get; set;}
    public PodProgress ProgressBeforeFault2 { get; set;}
    public int RadioGain { get; set;}
    public int Rssi { get; set;}
    public int? PulsesRemaining { get; set;}
    public int ActiveMinutes { get; set;}
    public int UnackedAlertsMask { get; set;}

    public ushort Unknown0 { get; set;}
    public int LastProgrammingCommandSequence { get; set;}

    public IMessageData FromParts(IMessageParts parts)
    {
        var data = parts.MainPart.Data;
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
        return this;
    }

    public IMessageParts ToParts()
    {
        throw new NotImplementedException();
    }
}
