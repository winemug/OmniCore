using OmniCore.Services.Interfaces;

namespace OmniCore.Services.Interfaces;

public interface IPodPacket
{
    int? Rssi { get; set; }
    uint Address { get; set; }
    PodPacketType Type { get; set; }
    int Sequence { get; set; }
    Bytes Data { get; set; }
    byte[] ToRadioData();
}