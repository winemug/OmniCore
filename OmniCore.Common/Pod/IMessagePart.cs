using OmniCore.Services.Interfaces.Entities;

namespace OmniCore.Services.Interfaces.Pod;

public interface IMessagePart
{
    bool RequiresNonce { get; }
    PodMessageType Type { get; set; }
    uint Nonce { get; set; }
    Bytes Data { get; set; }
}