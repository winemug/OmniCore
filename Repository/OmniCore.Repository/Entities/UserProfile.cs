using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Entities
{
    public class UserProfile : UpdateableEntity
    {
        [Indexed]
        public long UserId { get; set; }
        public long MedicationId { get; set; }

        public bool PodUseLocalTimeZone { get; set; }
        public bool PodBasalAdjustForLocalTimeZoneChanges { get; set; }
        public string PodTimezone { get; set; }
        public bool PodBasalAdjustForDstChanges { get; set; }

        public decimal[] PodBasalSchedule { get; set; }

        public decimal? PodAlertReservoirLow { get; set; }
        public decimal? PodAlertBeforeExpiry { get; set; }
        public bool PodAlertExpired { get; set; }
        public decimal? PodAlertBeforeAbsoluteExpiry { get; set; }

        public bool PodDeactivateOnError { get; set; }

        public bool PodBeepStartBolus { get; set; }
        public bool PodBeepEndBolus { get; set; }

        public bool PodBeepStartTempBasal { get; set; }
        public bool PodBeepEndTempBasal { get; set; }
        public decimal? PodBeepPeriodicTempBasalActive { get; set; }

        public bool PodBeepStartExtendedBolus { get; set; }
        public bool PodBeepEndExtendedBolus { get; set; }
        public decimal? PodBeepPeriodicExtendedBolusActive { get; set; }

    }
}
