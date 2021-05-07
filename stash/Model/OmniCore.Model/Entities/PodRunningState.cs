using System;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Entities
{
    public class PodRunningState
    {
        public DateTimeOffset? LastUpdated { get; set; }
        public DateTimeOffset? LastRadioContact { get; set; }

        public PodState State { get; set; }
        public DateTimeOffset? ActivationStart { get; set; }
        public DateTimeOffset? InsertionStart { get; set; }
        public decimal? PrimedVolumeDuringInsertion { get; set; }
        public DateTimeOffset? Started { get; set; }
        public DateTimeOffset? Stopped { get; set; }
        public DateTimeOffset? MarkedAsMalfunctioning { get; set; }
        public DateTimeOffset? Faulted { get; set; }

        public decimal DeliveredUnits { get; set; }
        public decimal ReservoirUnits { get; set; }
        public BasalState BasalState { get; set; }
        public DateTimeOffset? TempBasalStart { get; set; }
        public DateTimeOffset? TempBasalEnd { get; set; }
        public TimeSpan? TempBasalDuration { get; set; }
        public decimal? TempBasalRate { get; set; }
        public DateTimeOffset? SuspendBasalStart { get; set; }
        public DateTimeOffset? SuspendBasalEnd { get; set; }
        public BolusState BolusState { get; set; }
        public DateTimeOffset? BolusStart { get; set; }
        public DateTimeOffset? BolusEnd { get; set; }
        public decimal? BolusAmount { get; set; }
        public decimal? BolusDelivered { get; set; }
        public decimal? BolusRemaining { get; set; }
        public DateTimeOffset? ExtendedBolusStart { get; set; }
        public DateTimeOffset? ExtendedBolusEnd { get; set; }
        public TimeSpan? ExtendedBolusDuration { get; set; }
        public decimal? ExtendedBolusAmount { get; set; }
        public decimal? ExtendedBolusDelivered { get; set; }
        public decimal? ExtendedBolusRemaining { get; set; }
    }
}