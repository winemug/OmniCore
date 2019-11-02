using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Entities
{
    public class ExtendedBolus : UpdateableEntity
    {
        public long RequestId { get; set; }
        public decimal RequestedUnits { get; set; }
        public int RequestedDurationMinutes { get; set; }
        public DateTimeOffset? ActualDuration { get; set; }
        public DateTimeOffset Started { get; set; }
        public DateTimeOffset? Ended { get; set; }
        public bool? Canceled { get; set; }
        public decimal? DeliveredUnits { get; set; }
        public decimal? NotDeliveredUnits { get; set; }
    }
}
