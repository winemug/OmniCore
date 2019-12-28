using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Data.Entities
{
    public interface IPodResponseFaultAttributes
    {
        byte FaultCode { get; set; }
        int FaultTimeMinutes { get; set; }
        byte TableAccessFault { get; set; }
        byte InsulinStateTableCorr { get; set; }
        byte InternalFaultVars { get; set; }
        bool FaultWhileBolus { get; set; }
        PodProgress ProgressBeforeFault { get; set; }
        PodProgress ProgressBeforeFault2 { get; set; }
        byte[] FaultInformation2W { get; set; }
    }
}
