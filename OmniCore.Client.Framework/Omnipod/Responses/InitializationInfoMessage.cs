using OmniCore.Common.Pod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OmniCore.Framework.Omnipod.Responses;

public class InitializationInfoMessage : IMessageData
{
    public static Predicate<IMessageParts> CanParse =>
        (parts) =>
            parts.MainPart.Type == PodMessagePartType.ResponseInfo &&
            parts.MainPart.Data[0] == (byte)PodStatusType.Activation;

    public int FaultCode { get; set; }
    public int FaultMinute { get; set; }
    public uint Zero0 { get; set; }
    public uint Zero1 { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }

    public IMessageData FromParts(IMessageParts parts)
    {
        var data = parts.MainPart.Data;
        FaultCode = data[1];
        FaultMinute = data.Word(2);
        Zero0 = data.DWord(4);
        Zero1 = data.DWord(8);
        var month = data[12];
        var day = data[13];
        var year = data[14] + 2000;
        var hour = data[15];
        var minute = data[16];
        Date = new DateOnly(year, month, day);
        Time = new TimeOnly(hour, minute, 0);
        return this;
    }

    public IMessageParts ToParts()
    {
        throw new NotImplementedException();
    }
}
