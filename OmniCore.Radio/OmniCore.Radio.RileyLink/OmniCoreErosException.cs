using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
using OmniCore.Radio.RileyLink;

namespace OmniCore.Radio.RileyLink
{
    public class OmniCoreErosException : OmniCoreException
    {
        public RadioPacket ReceivedPacket = null;
        public PacketType? ExpectedType;
        public OmniCoreErosException(FailureType failureType, string message = "Unknown protocol error", RadioPacket received = null, PacketType? expectedType = null)
            : base(failureType, message)
        {
            this.ReceivedPacket = received;
            this.ExpectedType = expectedType;
        }
    }
}
