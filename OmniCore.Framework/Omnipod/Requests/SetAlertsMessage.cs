using OmniCore.Common.Pod;
using OmniCore.Services;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Framework.Omnipod.Requests;

public class SetAlertsMessage : IMessageData
{
    public List<AlertConfiguration> AlertConfigurations { get; set; }

    public static Predicate<IMessageParts> CanParse => (parts) => parts.MainPart.Type == PodMessagePartType.RequestConfigureAlerts;

    public SetAlertsMessage()
    {
        AlertConfigurations = new List<AlertConfiguration>();
    }

    public IMessageData FromParts(IMessageParts parts)
    {
        var idx = 0;
        while (idx < parts.MainPart.Data.Length)
        {
            var b0 = parts.MainPart.Data[idx + 0];
            var b1= parts.MainPart.Data[idx + 1];
            var u0 = parts.MainPart.Data.Word(idx + 2);
            var b2 = parts.MainPart.Data[idx + 4];
            var b3 = parts.MainPart.Data[idx + 5];

            var ac = new AlertConfiguration
            {
                AlertIndex = b0 >> 4,
                SetActive = (b0 & 0x08) > 0,
                ReservoirBased = (b0 & 0x04) > 0,
                SetAutoOff = (b0 & 0x02) > 0,
                AlertDurationMinutes = b1 + ((b0 & 0x01) << 8),
                AlertAfter = u0 & 0x3FFF,
                BeepPattern = (BeepPattern)b2,
                BeepType = (BeepType)b3
            };

            AlertConfigurations.Add(ac);
            idx += 6;
        }
        return this;
    }

    public IMessageParts ToParts()
    {
        var data = new Bytes();
        if (!AlertConfigurations.Any())
            throw new ApplicationException("Need to have at least one configuration");

        if (AlertConfigurations.DistinctBy(x => x.AlertIndex).Count() != 
            AlertConfigurations.Count())
            throw new ApplicationException("Alert indices need to be unique");

        foreach (var alertConfiguration in AlertConfigurations)
        {
            var b0 = (alertConfiguration.AlertIndex & 0x07) << 4;
            b0 |= alertConfiguration.SetActive ? 0x08 : 0x00;
            b0 |= alertConfiguration.ReservoirBased ? 0x04 : 0x00;
            b0 |= alertConfiguration.SetAutoOff ? 0x02 : 0x00;

            var d = alertConfiguration.AlertDurationMinutes & 0x1FF;
            b0 |= d >> 8;
            var b1 = d & 0xFF;
            data.Append((byte)b0).Append((byte)b1);

            var u0 = alertConfiguration.AlertAfter & 0x3FFF;
            var b2 = (byte)alertConfiguration.BeepPattern;
            var b3 = (byte)alertConfiguration.BeepType;
            data.Append((ushort)u0).Append(b2).Append(b3);
        }
        return new MessageParts(
            new MessagePart
            {
                Type = PodMessagePartType.RequestConfigureAlerts,
                RequiresNonce = true,
                Data = data
            }); ;
    }
}
