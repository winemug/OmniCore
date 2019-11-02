using OmniCore.Repository.Entities;
using System;

namespace OmniCore.Repository.Entities
{
    public class BasalSchedule : Entity
    {
        public long RequestId {get; set;}
        public int UtcOffset { get; set; }
        public int[] Schedule { get; set; }
        public DateTimeOffset PodDateTime { get; set; }
    }
}
