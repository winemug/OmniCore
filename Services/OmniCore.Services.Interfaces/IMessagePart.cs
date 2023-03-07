using OmniCore.Services.Interfaces;

namespace OmniCore.Services.Interfaces;

public interface IMessagePart
{
    bool RequiresNonce { get; }
    PodMessageType Type { get; set; }
    uint Nonce { get; set; }
    Bytes Data { get; set; }
}