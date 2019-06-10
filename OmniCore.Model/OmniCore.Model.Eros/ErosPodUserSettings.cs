using OmniCore.Model.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Eros
{
    public class ErosPodUserSettings : IPodUserSettings
    {
        [PrimaryKey, AutoIncrement]
        public long? Id { get; set; }
        public long ResultId { get; set; }

        public DateTime Created { get; set; }
        public Guid PodId { get; set; }
        public decimal? ReservoirWarningAtLevel { get; set; }
        public int? ExpiryWarningAtMinute { get; set; }

        public ErosPodUserSettings()
        {
            Created = DateTime.UtcNow;
        }
    }
}
