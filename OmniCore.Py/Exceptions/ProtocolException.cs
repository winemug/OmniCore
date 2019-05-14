namespace OmniCore.Py
{
    public class ProtocolException : OmnipyException
    {
        public Packet ReceivedPacket = null;
        public ProtocolException(string message = "Unknown protocol error", Packet received = null) : base(message)
        {
            this.ReceivedPacket = received;
        }
    }
}
