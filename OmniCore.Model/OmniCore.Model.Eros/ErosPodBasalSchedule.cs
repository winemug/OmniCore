using Newtonsoft.Json;
using OmniCore.Model.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Eros
{
    public class ErosPodBasalSchedule : IPodBasalSchedule
    {
        [PrimaryKey, AutoIncrement]
        public uint? Id { get; set; }

        public DateTime Created { get; set; }
        public Guid PodId { get; set; }
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

        public ErosPodBasalSchedule()
        {
            Created = DateTime.UtcNow;
        }

        public DateTime PodDateTime { get; set; }
        public DateTime Updated { get; set; }
    }
}
