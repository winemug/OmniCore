namespace OmniCore.Py
{
    public class OmnipyTimeoutError : OmnipyError
    {
        public OmnipyTimeoutError(string message = "Timeout error") : base(message) { }
    }
}
