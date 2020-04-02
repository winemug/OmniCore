using System;
using OmniCore.Radios.RileyLink.Enumerations;

namespace OmniCore.Radios.RileyLink.Protocol
{
    public class RileyLinkPacketResponse : RileyLinkResponse
    {
        public int Rssi { get; private set; }
        public byte[] PacketData { get; private set; }
        protected override void ParseResponse(byte[] responseData)
        {
            Rssi = (((int) responseData[0]) - 255) >> 2;
            PacketData = responseData[1..];
        }

        public IObservable<RileyLinkPacketResponse> AsObservable()
        {
            throw new NotImplementedException();
        }
    }
}