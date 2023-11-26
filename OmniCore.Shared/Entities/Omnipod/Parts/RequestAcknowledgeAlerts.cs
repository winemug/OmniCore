namespace OmniCore.Shared.Entities.Omnipod.Parts;

public class RequestAcknowledgeAlerts : IMessagePart
{
    public required bool[] AlertIndices { get; set; }

    public static IMessagePart ToInstance(Span<byte> span)
    {
        var indices = new bool[8];
        var d0 = span[0];
        for (var i = 0; i < 8; i++)
        {
            var im = 1 << i;
            indices[i] = (d0 & im) > 0;
        }

        return new RequestAcknowledgeAlerts
        { AlertIndices = indices };
    }

    public int ToBytes(Span<byte> span)
    {
        byte d0 = 0;
        for (var i = 0; i < 8; i++)
            if (AlertIndices[i])
                d0 |= (byte)(1 << i);
        span[0] = d0;
        return 1;
    }
}