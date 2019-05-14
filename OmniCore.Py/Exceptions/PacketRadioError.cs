namespace OmniCore.Py
{
    using System;

    public class PacketRadioError : OmnipyError
    {
        public PacketRadioError(string message = "Unknown RL error", Exception innerException = null) : base(message, innerException)
        {
        }
    }
}
