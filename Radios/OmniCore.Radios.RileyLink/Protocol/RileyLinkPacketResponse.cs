using System;

namespace OmniCore.Radios.RileyLink.Protocol
{
    public class RileyLinkPacketResponse : RileyLinkStandardResponse
    {
        public int Rssi { get; private set; }
        public byte[] PacketData { get; private set; }

        protected override void ParseInternal(byte[] responseData)
        {
            Rssi = (responseData[0] - 255) >> 2;
            PacketData = responseData[1..];
        }

        public IObservable<RileyLinkPacketResponse> AsObservable()
        {
            throw new NotImplementedException();
        }
    }
}