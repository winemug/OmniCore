using System;
using OmniCore.Model.Enums;

namespace OmniCore.Model.Interfaces
{
    public interface IStatus
    {
        long? Id { get; set; }
        Guid PodId { get; set; }
        DateTimeOffset Created { get; set; }

        decimal? NotDeliveredInsulin { get; set; }
        decimal? DeliveredInsulin { get; set; }
        decimal? Reservoir { get; set; }

        bool? Faulted { get; set; }
        PodProgress? Progress { get; set; }
        BasalState? BasalState { get; set; }
        BolusState? BolusState { get; set; }

        uint? ActiveMinutes { get; set; }
        byte? AlertMask { get; set; }

        decimal? DeliveredInsulinEstimate { get; set; }
        decimal? ReservoirEstimate { get; set; }
        uint? ActiveMinutesEstimate { get; set; }
        BasalState? BasalStateEstimate { get; set; }
        BolusState? BolusStateEstimate { get; set; }
        decimal? TemporaryBasalTotalHours { get; set; }
        TimeSpan? TemporaryBasalRemaining { get; set; }
        decimal? TemporaryBasalRate { get; set; }
        decimal? ScheduledBasalRate { get; set; }
        decimal? ScheduledBasalAverage { get; set; }

        void UpdateWithEstimates(IPod pod);
    }
}
