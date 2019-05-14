namespace OmniCore.Py
{
    public class ProtocolError : OmnipyError
    {
        public RadioPacket ReceivedPacket = null;
        public ProtocolError(string message = "Unknown protocol error", RadioPacket received = null) : base(message)
        {
            this.ReceivedPacket = received;
        }
    }
}
