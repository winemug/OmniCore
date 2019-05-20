using OmniCore.Model.Exceptions;
using System;

namespace OmniCore.Radio.RileyLink
{
    public class PacketRadioException : OmniCoreException
    {
        public PacketRadioException(string message = "Unknown RL error", Exception innerException = null) : base(message, innerException)
        {
        }
    }
}
