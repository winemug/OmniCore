using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Entities
{
    public class PodResponseFault
    {
        public byte FaultCode { get; set; }
        public int FaultTimeMinutes { get; set; }
        public byte TableAccessFault { get; set; }
        public byte InsulinStateTableCorr { get; set; }
        public byte InternalFaultVars { get; set; }
        public bool FaultWhileBolus { get; set; }
        public PodProgress ProgressBeforeFault { get; set; }
        public PodProgress ProgressBeforeFault2 { get; set; }
        public byte[] FaultInformation2W { get; set; }
    }
}