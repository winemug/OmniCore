using System;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using SQLite;

namespace OmniCore.Repository.Sqlite.Entities
{
    public class MedicationDeliveryEntity : Entity, IMedicationDeliveryEntity
    {
        [Ignore]
        public IUserEntity User { get; set; }
        public long? UserId { get; set; }

        [Ignore]
        public IMedicationEntity Medication { get; set; }
        public long? MedicationId { get; set; }
        
        [Ignore]
        public IPodEntity Pod { get; set; }
        public long? PodId { get; set; }

        public DateTimeOffset? DeliveryStart { get; set; }
        public TimeSpan? IntendedDuration { get; set; }
        public decimal? IntendedAmount { get; set; }
        public DateTimeOffset? DeliveryEnded { get; }
        public decimal? DeliveredAmount { get; set; }
    }
}