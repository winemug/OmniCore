using OmniCore.Repository.Entities;
using SQLite;
using System;

namespace OmniCore.Repository.Entities
{
    public class BasalSchedule : Entity
    {
        [Indexed]
        public long RequestId {get; set;}
        public int UtcOffset { get; set; }
        public decimal[] Schedule { get; set; }
        public DateTimeOffset PodDateTime { get; set; }
    }
}
