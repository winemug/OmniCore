using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class MessagePart : IMessagePart
{
    public bool RequiresNonce { get; set;  }
    public PodMessagePartType Type { get; set; }
    public uint Nonce { get; set; }
    public Bytes Data { get; set; }

    public override string ToString()
    {
        return $"Type: {Type:X} Data: {Data}";
    }
}