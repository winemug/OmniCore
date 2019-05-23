using OmniCore.Model.Exceptions;

namespace OmniCore.Radio.RileyLink
{
    public class ErosProtocolException : OmniCoreException
    {
        public RadioPacket ReceivedPacket = null;
        public ErosProtocolException(string message = "Unknown protocol error", RadioPacket received = null) : base(message)
        {
            this.ReceivedPacket = received;
        }
    }
}
