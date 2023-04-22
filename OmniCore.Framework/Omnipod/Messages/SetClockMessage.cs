using OmniCore.Common.Pod;
using OmniCore.Services;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
using System;

namespace OmniCore.Framework.Omnipod.Messages;

public class SetClockMessage : IMessageData
{
    public static Predicate<IMessageParts> CanParse => (parts) => parts.MainPart.Type == PodMessagePartType.RequestSetupPod;

    public int PacketTimeout { get; set; } = 50;
    public uint RadioAddress { get; set; }
    public uint Lot { get; set; }
    public uint Serial { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }

    public IMessageData FromParts(IMessageParts parts)
    {
        var data = parts.MainPart.Data;
        PacketTimeout = data[1];

        var month = data[2];
        var day = data[3];
        var year = data[4] + 2000;
        Date = new DateOnly(year, month, day);

        var hour = data[5];
        var minute = data[6];
        Time = new TimeOnly(hour, minute, 0);

        Lot = data.DWord(7);
        Serial = data.DWord(11);
        return this;
    }

    public IMessageParts ToParts()
    {
        var data = new Bytes(RadioAddress).Append(0);

        if (PacketTimeout > 50)
            data.Append(50);
        else
            data.Append((byte)PacketTimeout);

        data.Append((byte)Date.Month).Append((byte)Date.Day).Append((byte)(Date.Year - 2000))
            .Append((byte)Time.Hour).Append((byte)Time.Minute);

        data.Append(Lot).Append(Serial);

        return new MessageParts(
            new MessagePart
            {
                Type = PodMessagePartType.RequestSetupPod,
                RequiresNonce = false,
                Data = data,
            });
    }
}
