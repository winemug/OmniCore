namespace OmniCore.Model.Exceptions
{
    public class OmniCoreTimeoutException : OmniCoreException
    {
        public OmniCoreTimeoutException(string message = "Timeout error") : base(message) { }
    }
}
