namespace OmniCore.Common.Pod;

public class BlePacketExchangeResult
{
    public IPodPacket Sent { get; set; }
    public IPodPacket? Received { get; set; }
    public bool BleConnectionSuccessful { get; set; }
}