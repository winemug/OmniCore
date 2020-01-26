using System;

namespace OmniCore.Model.Interfaces.Platform.Common.Data.Entities
{
    public interface IMedicationDeliveryEntity
    {
        IUser User { get; set; }
        IMedicationEntity Medication { get; set; }
        IPod Pod { get; set; }

        DateTimeOffset? DeliveryStart { get; set; }
        TimeSpan? IntendedDuration { get; set; }
        decimal? IntendedAmount { get; set; }
        
        DateTimeOffset? DeliveryEnded { get; }
        decimal? DeliveredAmount { get; set; }
    }
}