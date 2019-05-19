namespace OmniCore.Model.Exceptions
{
    using System;

    public class PacketRadioException : OmniCoreException
    {
        public PacketRadioException(string message = "Unknown RL error", Exception innerException = null) : base(message, innerException)
        {
        }
    }
}
