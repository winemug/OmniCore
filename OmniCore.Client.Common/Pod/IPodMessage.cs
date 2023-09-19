using OmniCore.Common.Entities;

namespace OmniCore.Common.Pod;

public interface IPodMessage
{
    uint Address { get; }
    int Sequence { get; }
    bool Critical { get; }
    Bytes Body { get; }
    IMessageData? Data { get; }
}