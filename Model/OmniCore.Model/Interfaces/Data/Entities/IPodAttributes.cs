using System;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Data.Entities
{
    public interface IPodAttributes
    {
        IBasalScheduleAttributes BasalSchedule { get; set; }
        Guid? UniqueId { get; set; }
        string HwRevision { get; set; }
        string SwRevision { get; set; }

        TimeSpan PodUtcOffset { get; set; }
        bool AutoAdjustPodTime { get; set; }
        bool DeactivateOnError { get; set; }

        PodState State { get; set; }
        DateTimeOffset? ActivationStart { get; set; }
        DateTimeOffset? InsertionStart { get; set; }
        DateTimeOffset? Started { get; set; }
        DateTimeOffset? Stopped { get; set; }
        DateTimeOffset? Faulted { get; set; }


        decimal DeliveredUnits { get; set; }
        decimal ReservoirUnits { get; set; }
        BasalState BasalState { get; set; }

        DateTimeOffset? TempBasalStart { get; set; }
        DateTimeOffset? TempBasalEnd { get; set; }
        TimeSpan? TempBasalDuration { get; set; }
        decimal? TempBasalRate { get; set; }

        DateTimeOffset? SuspendBasalStart { get; set; }
        DateTimeOffset? SuspendBasalEnd { get; set; }

        BolusState BolusState { get; set; }
        DateTimeOffset? BolusStart { get; set; }
        DateTimeOffset? BolusEnd { get; set; }
        decimal? BolusAmount { get; set; }
        decimal? BolusDelivered { get; set; }
        decimal? BolusRemaining { get; set; }

        DateTimeOffset? ExtendedBolusStart { get; set; }
        DateTimeOffset? ExtendedBolusEnd { get; set; }
        TimeSpan? ExtendedBolusDuration { get; set; }
        decimal? ExtendedBolusAmount { get; set; }
        decimal? ExtendedBolusDelivered { get; set; }
        decimal? ExtendedBolusRemaining { get; set; }

        // eros specific
        uint RadioAddress { get; set; }

    }
}
