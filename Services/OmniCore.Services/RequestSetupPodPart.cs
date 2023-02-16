using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class RequestSetupPodPart : MessagePart
{
    public override bool RequiresNonce => false;
    public override RadioMessageType Type => RadioMessageType.RequestSetupPod;

    public RequestSetupPodPart(uint radioAddress, uint lot, uint serial,
        int packetTimeout,
        int year, int month, int day, int hour, int minute)
    {
        var b = new Bytes(radioAddress).Append((byte)0);
        if (packetTimeout > 50)
            b.Append((byte)50);
        else
            b.Append((byte)packetTimeout);
        b.Append((byte)month).Append((byte)day).Append((byte)(year - 2000))
            .Append((byte)hour).Append((byte)minute);
        b.Append(lot).Append(serial);
        Data = b;
    }
}