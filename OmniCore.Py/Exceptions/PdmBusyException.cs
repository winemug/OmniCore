namespace OmniCore.Py
{
    public class PdmBusyException : PdmException
    {
        public PdmBusyException(string message = "Pdm is busy.") : base(message) { }
    }
}
