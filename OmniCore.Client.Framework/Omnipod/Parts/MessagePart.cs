using OmniCore.Common.Entities;
using OmniCore.Common.Pod;

namespace OmniCore.Framework.Omnipod.Parts;

public class MessagePart : IMessagePart
{
    public virtual bool RequiresNonce { get; set; }
    public virtual PodMessagePartType Type { get; set; }
    public uint Nonce { get; set; }
    public Bytes Data { get; set; }

    public override string ToString()
    {
        return $"Type: {Type:X} Data: {Data}";
    }
}