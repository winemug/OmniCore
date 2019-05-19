namespace OmniCore.Model.Exceptions
{
    using System;

    public class PdmException : OmniCoreException
    {
        public PdmException(string message = "Unknown pdm error", Exception innerException = null) : base(message, innerException)
        {
        }
    }
}
