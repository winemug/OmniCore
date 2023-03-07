using System.Collections.Generic;
using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public interface IPodMessage
{
    uint Address { get; set; }
    int Sequence { get; set; }
    bool WithCriticalFollowup { get; set; }
    List<IMessagePart> Parts { get; set; }
    uint? AckAddressOverride { get; set; }
    Bytes Body { get; set; }
    Bytes GetBody();
}