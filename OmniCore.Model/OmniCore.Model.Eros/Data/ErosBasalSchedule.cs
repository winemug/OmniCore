using Newtonsoft.Json;
using OmniCore.Model.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Eros.Data
{
    public class ErosBasalSchedule : IBasalSchedule
    {
        [PrimaryKey, AutoIncrement]
        public long? Id { get; set; }
        public Guid PodId { get; set; }
        public DateTimeOffset Created { get; set; }

        public int UtcOffset { get; set; }

        [Ignore]
        public decimal[] BasalSchedule { get; set; }

        public string BasalScheduleJson
        {
            get
            {
                return JsonConvert.SerializeObject(BasalSchedule);
            }
            set
            {
                BasalSchedule = JsonConvert.DeserializeObject<decimal[]>(value);
            }
        }

        public DateTimeOffset PodDateTime { get; set; }
    }
}
