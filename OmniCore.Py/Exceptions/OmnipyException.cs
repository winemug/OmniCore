namespace OmniCore.Py
{
    using System;

    public class OmnipyException : Exception
    {
        public OmnipyException(string message = "Unknown", Exception inner = null) : base(message, inner) { }
    }
}
