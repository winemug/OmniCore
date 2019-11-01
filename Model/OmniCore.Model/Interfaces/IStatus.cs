using System;
using OmniCore.Model.Enums;

namespace OmniCore.Model.Interfaces
{
    public interface IStatus
    {
        long? Id { get; set; }
        long RequestId {get; set;}
        Guid PodId { get; set; }
        DateTimeOffset Created { get; set; }

        decimal? NotDeliveredInsulin { get; set; }
        decimal? DeliveredInsulin { get; set; }
        decimal? Reservoir { get; set; }

        bool? Faulted { get; set; }
        PodProgress Progress { get; set; }
        BasalState? BasalState { get; set; }
        BolusState? BolusState { get; set; }

        uint? ActiveMinutes { get; set; }
        byte? AlertMask { get; set; }
 

    }
}
