namespace OmniCore.Model.Exceptions
{
    using System;

    public class OmniCoreException : Exception
    {
        public OmniCoreException(string message = "Unknown", Exception inner = null) : base(message, inner) { }
    }
}
