namespace OmniCore.Py
{
    using System;

    public class OmnipyError : Exception
    {
        public OmnipyError(string message = "Unknown", Exception inner = null) : base(message, inner) { }
    }

    public class PacketRadioError : OmnipyError
    {
        private int? err_code;
        public PacketRadioError(string message = "Unknown RL error", int? err_code = null) : base(message)
        {
            this.err_code = err_code;
        }
    }

    public class ProtocolError : OmnipyError
    {
        public RadioPacket ReceivedPacket = null;
        public ProtocolError(string message = "Unknown protocol error", RadioPacket received = null) : base(message)
        {
            this.ReceivedPacket = received;
        }
    }

    public class OmnipyTimeoutError : OmnipyError
    {
        public OmnipyTimeoutError(string message = "Timeout error") : base(message) { }
    }

    public class PdmError : OmnipyError
    {
        public PdmError(string message = "Unknown pdm error", Exception inner = null) : base(message, inner)
        {
        }
    }

    public class PdmBusyError : PdmError
    {
        public PdmBusyError(string message = "Pdm is busy.") : base(message) { }
    }
}
