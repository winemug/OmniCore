using System;

namespace OmniCore.Model.Entities
{
    public class MedicationDeliveryEntity : Entity
    {
        public UserEntity User { get; set; }
        public MedicationEntity Medication { get; set; }
        public PodEntity Pod { get; set; }

        public DateTimeOffset? DeliveryStart { get; set; }
        public TimeSpan? IntendedDuration { get; set; }
        public decimal? IntendedAmount { get; set; }
        public DateTimeOffset? DeliveryEnded { get; }
        public decimal? DeliveredAmount { get; set; }
    }
}