using OmniCore.Common.Entities;
using OmniCore.Common.Pod;
using OmniCore.Framework.Omnipod.Parts;

namespace OmniCore.Framework.Omnipod.Requests;

public class SetClockMessage : IMessageData
{
    public int PacketTimeout { get; set; } = 50;
    public uint RadioAddress { get; set; }
    public uint Lot { get; set; }
    public uint Serial { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }

    public static Predicate<IMessageParts> CanParse =>
        parts => parts.MainPart.Type == PodMessagePartType.RequestSetupPod;

    public IMessageData FromParts(IMessageParts parts)
    {
        var data = parts.MainPart.Data;
        PacketTimeout = data[5];

        var month = data[6];
        var day = data[7];
        var year = data[8] + 2000;
        Date = new DateOnly(year, month, day);

        var hour = data[9];
        var minute = data[10];
        Time = new TimeOnly(hour, minute, 0);

        Lot = data.DWord(11);
        Serial = data.DWord(12);
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
                Data = data
            });
    }
}