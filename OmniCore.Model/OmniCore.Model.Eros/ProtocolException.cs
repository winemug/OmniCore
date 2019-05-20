using OmniCore.Model.Exceptions;

namespace OmniCore.Model.Eros
{
    public class ProtocolException : OmniCoreException
    {
        public Packet ReceivedPacket = null;
        public ProtocolException(string message = "Unknown protocol error", Packet received = null) : base(message)
        {
            this.ReceivedPacket = received;
        }
    }
}
