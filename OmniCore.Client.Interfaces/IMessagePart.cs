using OmniCore.Common.Entities;

namespace OmniCore.Common.Pod;

public interface IMessagePart
{
    bool RequiresNonce { get; }
    PodMessagePartType Type { get; set; }
    uint Nonce { get; set; }
    Bytes Data { get; set; }
}