using System.Collections.Generic;
using OmniCore.Common.Pod;
using OmniCore.Services.Interfaces.Entities;

namespace OmniCore.Services.Interfaces.Pod;

public interface IPodMessage
{
    uint Address { get; set; }
    int Sequence { get; set; }
    bool WithCriticalFollowup { get; set; }
    IMessageParts Parts { get; set; }
    uint? AckAddressOverride { get; set; }
    Bytes Body { get; set; }
    Bytes GetBody();
}