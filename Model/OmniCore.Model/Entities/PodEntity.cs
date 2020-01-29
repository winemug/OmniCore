using System;
using System.Collections.Generic;
using System.Text;
using Innofactor.EfCoreJsonValueConverter;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Entities
{
    public class PodEntity : Entity
    {
        public UserEntity User { get; set; }
        public MedicationEntity Medication { get; set; }
        // public TherapyProfileEntity TherapyProfile { get; set; }
        // public BasalScheduleEntity ReferenceBasalSchedule { get; set; }
        public RadioEntity Radio { get; set; }
        //public BasalSchedule BasalSchedule { get; set; }

        [JsonField]
        public PodOptions Options { get; set; } = new PodOptions();
        
        [JsonField]
        public ReminderSettings ExpiresSoonReminder { get; set; }
        [JsonField]
        public ReminderSettings ReservoirLowReminder { get; set; }
        [JsonField]
        public ReminderSettings ExpiredReminder { get; set; }

        public uint RadioAddress { get; set; }
        public uint Lot { get; set; }
        public uint Serial { get; set; }

        public string HwRevision { get; set; }
        public string SwRevision { get; set; }

        public TimeSpan PodUtcOffset { get; set; }
    }
}
