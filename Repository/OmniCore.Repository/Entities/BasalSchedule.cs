using Newtonsoft.Json;
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
        [Ignore]
        public decimal[] Schedule
        {
            get
            {
                if (String.IsNullOrEmpty(ScheduleJson))
                    return null;
                else
                    return JsonConvert.DeserializeObject<decimal[]>(ScheduleJson);
            }
            set
            {
                if (value == null)
                    ScheduleJson = null;
                else
                    ScheduleJson = JsonConvert.SerializeObject(value);
            }
        }

        public string ScheduleJson { get; set; }
        public DateTimeOffset PodDateTime { get; set; }
    }
}
