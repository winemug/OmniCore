using System;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;

namespace OmniCore.Services;

public class ResponseInfoActivationPart : ResponseInfoPart
{
    public ResponseInfoActivationPart(Bytes data)
    {
        FaultCode = data[1];
        FaultMinute = data.Word(2);
        Zero0 = data.DWord(4);
        Zero1 = data.DWord(8);
        var month = data[12];
        var day = data[13];
        var year = data[14];
        var hour = data[15];
        var minute = data[16];
        InitializationDate = new DateTimeOffset(year + 2000, month, day, hour, minute, 0, TimeSpan.Zero);
        Data = data;
    }

    public override RequestStatusType StatusType => RequestStatusType.Activation;

    public int FaultCode { get; }
    public int FaultMinute { get; }
    public uint Zero0 { get; }
    public uint Zero1 { get; }
    public DateTimeOffset InitializationDate { get; }
}