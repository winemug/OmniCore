namespace OmniCore.Model.Exceptions
{
    public class PdmBusyException : PdmException
    {
        public PdmBusyException(string message = "Pdm is busy.") : base(message) { }
    }
}
