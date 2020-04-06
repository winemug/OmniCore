using System;
using System.Collections.Generic;
using Innofactor.EfCoreJsonValueConverter;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Entities
{
    public class PodEntity : Entity
    {
        public PodType Type { get; set; }
        public UserEntity User { get; set; }
        public MedicationEntity Medication { get; set; }

        // public TherapyProfileEntity TherapyProfile { get; set; }
        // public BasalScheduleEntity ReferenceBasalSchedule { get; set; }
        public ICollection<PodRadioEntity> PodRadios { get; set; }
        //public BasalSchedule BasalSchedule { get; set; }

        [JsonField] public ErosPodOptions Options { get; set; } = new ErosPodOptions();

        [JsonField] public ReminderSettings ExpiresSoonReminder { get; set; }

        [JsonField] public ReminderSettings ReservoirLowReminder { get; set; }

        [JsonField] public ReminderSettings ExpiredReminder { get; set; }

        public uint RadioAddress { get; set; }
        public uint Lot { get; set; }
        public uint Serial { get; set; }

        public string HwRevision { get; set; }
        public string SwRevision { get; set; }

        public TimeSpan PodUtcOffset { get; set; }
    }
}