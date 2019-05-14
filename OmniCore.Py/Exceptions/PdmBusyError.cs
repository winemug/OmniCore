namespace OmniCore.Py
{
    public class PdmBusyError : PdmError
    {
        public PdmBusyError(string message = "Pdm is busy.") : base(message) { }
    }
}
