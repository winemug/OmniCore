using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using SQLite;

namespace OmniCore.Eros
{
    public class ErosPod : IPod
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        public string[] ProviderSpecificRadioIds { get; set; }
        public uint? Lot { get; set; }
        public uint? Serial { get; set; }
        public uint RadioAddress { get; set; }

        private int _messageSequence;
        public int MessageSequence
        {
            get => _messageSequence;
            set => _messageSequence = value % 16;
        }
        public DateTimeOffset? ActivationDate { get; set; }
        public DateTimeOffset? InsertionDate { get; set; }
        public string VersionPi { get; set; }
        public string VersionPm { get; set; }
        public string VersionUnknown { get; set; }
        public decimal? ReservoirUsedForPriming { get; set; }
        public decimal TotalDelivered { get; set; }
        public decimal Reservoir { get; set; }
        public uint ActiveMinutes { get; set; }
        public PodProgress Progress { get; set; }
        public BasalState BasalState { get; set; }
        public DateTimeOffset? TempBasalStart { get; set; }
        public DateTimeOffset? TempBasalEnd { get; set; }
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
        public decimal? ExtendedBolusAmount { get; set; }
        public decimal? ExtendedBolusDelivered { get; set; }
        public decimal? ExtendedBolusRemaining { get; set; }
        public bool Archived { get; set; }
        public bool Faulted { get; set; }
        public PodFault FaultCode { get; set; }
        public DateTimeOffset? FaultDate { get; set; }

        public IBasalSchedule BasalSchedule { get; set; }
        public IReminderConfiguration[] Reminders { get; set; }
        public decimal? TempBasalDurationInHours { get; set; }
    }
}
