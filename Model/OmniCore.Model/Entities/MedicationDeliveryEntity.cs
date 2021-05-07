using System;

namespace OmniCore.Model.Entities
{
    public class MedicationDeliveryEntity : Entity
    {
        public PodEntity Pod { get; set; }

        public DateTime? DeliveryStart { get; set; }
        public TimeSpan? IntendedDuration { get; set; }
        public decimal? IntendedAmount { get; set; }
        public DateTime? DeliveryEnded { get; }
        public decimal? DeliveredAmount { get; set; }
    }
}