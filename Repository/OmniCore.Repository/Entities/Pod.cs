using System;
using System.Threading.Tasks;
using OmniCore.Repository.Enums;
using SQLite;

namespace OmniCore.Repository.Entities
{
    public class Pod : UpdateableEntity
    {
        [Indexed]
        public long UserProfileId { get; set; }
        public Guid? PodUniqueId { get; set; }

        [Indexed]
        public bool Archived { get; set; }

        public string ProviderSpecificRadioIds { get; set; }

        public uint? Lot { get; set; }
        public uint? Serial { get; set; }
        public uint RadioAddress { get; set; }

        private int _messageSequence;
        public int MessageSequence
        {
            get => _messageSequence;
            set => _messageSequence = value % 16;
        }

        private int _packetSequence;
        public int PacketSequence
        {
            get => _packetSequence;
            set => _packetSequence = value % 32;
        }

        public uint? LastNonce { get; set; }
        public int NoncePtr { get; set; }
        public int NonceRuns { get; set; }
        public uint NonceSeed { get; set; }
        public uint? NonceSync { get; set; }

        public string VersionPi { get; set; }
        public string VersionPm { get; set; }
        public string VersionUnknown { get; set; }

        public bool Faulted { get; set; }
        public PodFault FaultCode { get; set; }
        public DateTimeOffset? FaultDate { get; set; }

        public DateTimeOffset? ActivationDate { get; set; }
        public DateTimeOffset? InsertionDate { get; set; }
        public decimal? ReservoirUsedForPriming { get; set; }

        public decimal TotalDeliveredUnits { get; set; }
        public decimal ReservoirUnits { get; set; }
        public uint ActiveMinutes { get; set; }

        public PodProgress Progress { get; set; }

        public BasalState BasalState { get; set; }

        public DateTimeOffset? TempBasalStart { get; set; }
        public DateTimeOffset? TempBasalEnd { get; set; }
        public decimal? TempBasalDurationInHours { get; set; }
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

        [Ignore]
        public BasalSchedule BasalSchedule { get; set; }

        [Ignore]
        public ReminderConfiguration[] Reminders { get; set; }

        public decimal? AlertReservoirLow { get; set; }
        public decimal? AlertBeforeExpiry { get; set; }
        public bool AlertExpired { get; set; }
        public decimal? AlertBeforeAbsoluteExpiry { get; set; }

        public bool DeactivateOnError { get; set; }

        public bool BeepStartBolus { get; set; }
        public bool BeepEndBolus { get; set; }

        public bool BeepStartTempBasal { get; set; }
        public bool BeepEndTempBasal { get; set; }
        public decimal? BeepPeriodicTempBasalActive { get; set; }

        public bool BeepStartExtendedBolus { get; set; }
        public bool BeepEndExtendedBolus { get; set; }
        public decimal? BeepPeriodicExtendedBolusActive { get; set; }

        public bool UseLocalTimeZone { get; set; }
        public bool BasalAdjustForLocalTimeZoneChanges { get; set; }
        public string Timezone { get; set; }
        public bool BasalAdjustForDstChanges { get; set; }
    }
}