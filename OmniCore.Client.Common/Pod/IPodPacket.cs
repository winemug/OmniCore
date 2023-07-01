using OmniCore.Common.Entities;

namespace OmniCore.Common.Pod;

public interface IPodPacket
{
    int? Rssi { get; set; }
    uint Address { get; set; }
    PodPacketType Type { get; set; }
    int Sequence { get; set; }
    Bytes Data { get; set; }
    Bytes ToRadioData();
}