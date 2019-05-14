namespace OmniCore.Py
{
    using System;

    public class PdmException : OmnipyException
    {
        public PdmException(string message = "Unknown pdm error", Exception innerException = null) : base(message, innerException)
        {
        }
    }
}
