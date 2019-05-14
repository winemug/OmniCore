namespace OmniCore.Py
{
    using System;

    public class PdmError : OmnipyError
    {
        public PdmError(string message = "Unknown pdm error", Exception innerException = null) : base(message, innerException)
        {
        }
    }
}
