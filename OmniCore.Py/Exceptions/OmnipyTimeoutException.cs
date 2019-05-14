namespace OmniCore.Py
{
    public class OmnipyTimeoutException : OmnipyException
    {
        public OmnipyTimeoutException(string message = "Timeout error") : base(message) { }
    }
}
