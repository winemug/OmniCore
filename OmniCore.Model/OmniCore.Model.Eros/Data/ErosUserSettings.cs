using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Eros.Data
{
    public class ErosUserSettings : IUserSettings
    {
        [PrimaryKey, AutoIncrement]
        public long? Id { get; set; }
        public Guid PodId { get; set; }
        public DateTime Created { get; set; }

        public decimal? ReservoirWarningAtLevel { get; set; }
        public int? ExpiryWarningAtMinute { get; set; }

        public ErosUserSettings()
        {
        }
    }
}
