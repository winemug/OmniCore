using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class RequestConfigureAlertsPart : MessagePart
{
    public override bool RequiresNonce => true;
    public override PodMessageType Type => PodMessageType.RequestConfigureAlerts;

    public RequestConfigureAlertsPart(AlertConfiguration[] alertConfigurations)
    {
        Data = new Bytes();
        foreach (var alertConfiguration in alertConfigurations)
        {
            var b0 = (alertConfiguration.AlertIndex & 0x07) << 4;
            b0 |= alertConfiguration.SetActive ? 0x08 : 0x00;
            b0 |= alertConfiguration.ReservoirBased ? 0x04 : 0x00;
            b0 |= alertConfiguration.SetAutoOff ? 0x02 : 0x00;

            var d = alertConfiguration.AlertDurationMinutes & 0x1FF;
            b0 |= d >> 8;
            var b1 = d & 0xFF;
            Data.Append((byte)b0).Append((byte)b1);

            var u0 = alertConfiguration.AlertAfter & 0x3FFF;
            var b2 = (byte)alertConfiguration.BeepPattern;
            var b3 = (byte)alertConfiguration.BeepType;
            Data.Append((ushort)u0).Append(b2).Append(b3);
        }
    }
}