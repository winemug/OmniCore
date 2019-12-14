using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Repository.Model
{
    public class Pod : IPodEntity
    {
        public IBasalScheduleAttributes BasalSchedule { get; set; }
        public Guid? UniqueId { get; set; }
        public string HwRevision { get; set; }
        public string SwRevision { get; set; }
        public TimeSpan PodUtcOffset { get; set; }
        public bool AutoAdjustPodTime { get; set; }
        public bool DeactivateOnError { get; set; }
        public PodState State { get; set; }
        public DateTimeOffset? ActivationStart { get; set; }
        public DateTimeOffset? InsertionStart { get; set; }
        public DateTimeOffset? Started { get; set; }
        public DateTimeOffset? Stopped { get; set; }
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
        public IReminderAttributes ExpiresSoonReminder { get; set; }
        public IReminderAttributes ReservoirLowReminder { get; set; }
        public IReminderAttributes ExpiredReminder { get; set; }
        public bool BeepStartBolus { get; set; }
        public bool BeepEndBolus { get; set; }
        public bool BeepStartTempBasal { get; set; }
        public bool BeepEndTempBasal { get; set; }
        public decimal? BeepWhileTempBasalActive { get; set; }
        public bool BeepStartExtendedBolus { get; set; }
        public bool BeepEndExtendedBolus { get; set; }
        public decimal? BeepWhileExtendedBolusActive { get; set; }
        public long Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public bool IsDeleted { get; set; }
        public IExtendedAttribute ExtendedAttribute { get; set; }
        public IUserEntity User { get; set; }
        public IMedicationEntity Medication { get; set; }
        public ITherapyProfileEntity TherapyProfile { get; set; }
        public IList<IRadioEntity> Radios { get; set; }
    }
}
