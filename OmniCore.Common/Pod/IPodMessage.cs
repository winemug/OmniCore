using System.Collections.Generic;
using OmniCore.Common.Pod;
using OmniCore.Services.Interfaces.Entities;

namespace OmniCore.Services.Interfaces.Pod;

public interface IPodMessage
{
    uint Address { get; }
    int Sequence { get; }
    bool Critical { get; }
    Bytes Body { get; }
    IMessageData Data { get; }
}