namespace OmniCore.Py
{
    using System;

    public class OmnipyError : Exception
    {
        public OmnipyError(string message = "Unknown", Exception inner = null) : base(message, inner) { }
    }
}
