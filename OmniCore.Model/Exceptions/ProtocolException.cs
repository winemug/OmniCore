namespace OmniCore.Model.Exceptions
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
