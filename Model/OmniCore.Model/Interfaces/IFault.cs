using System;
using OmniCore.Model.Enums;

namespace OmniCore.Model.Interfaces
{
    public interface IFault
    {
        long? Id { get; set; }
        Guid PodId { get; set; }
        DateTimeOffset Created { get; set; }

        int? FaultCode { get; set; }
        int? FaultRelativeTime { get; set; }
        bool? FaultedWhileImmediateBolus { get; set; }
        uint? FaultInformation2LastWord { get; set; }
        int? InsulinStateTableCorruption { get; set; }
        int? InternalFaultVariables { get; set; }
        PodProgress? ProgressBeforeFault { get; set; }
        PodProgress? ProgressBeforeFault2 { get; set; }
        int? TableAccessFault { get; set; }
    }
}
