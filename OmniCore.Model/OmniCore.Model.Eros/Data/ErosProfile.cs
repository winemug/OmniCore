using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces;

namespace OmniCore.Model.Eros.Data
{
    public class ErosProfile : IProfile
    {
        [PrimaryKey, AutoIncrement]
        public long? Id { get; set; }
        public DateTimeOffset Created { get; set; }

        public string Name { get; set; }

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

        public int UtcOffset { get; set; }
    }
}
