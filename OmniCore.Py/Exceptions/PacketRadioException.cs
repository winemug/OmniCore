namespace OmniCore.Py
{
    using System;

    public class PacketRadioException : OmnipyException
    {
        public PacketRadioException(string message = "Unknown RL error", Exception innerException = null) : base(message, innerException)
        {
        }
    }
}
