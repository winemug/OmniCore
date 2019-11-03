using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Entities
{
    public class Bolus
    {
        [Indexed]
        public long RequestId { get; set; }
        public decimal RequestedUnits { get; set; }
        public decimal? DeliveredUnits { get; set; }
        public decimal? NotDeliveredUnits { get; set; }
        public DateTimeOffset Started { get; set; }
        public DateTimeOffset? Finished { get; set; }
        public bool? Stopped { get; set; }
    }
}
