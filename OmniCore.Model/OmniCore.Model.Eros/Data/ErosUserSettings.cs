using OmniCore.Model.Interfaces;
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
        public DateTimeOffset Created { get; set; }

        public decimal? ReservoirWarningAtLevel { get; set; }
        public int? ExpiryWarningAtMinute { get; set; }

        public ErosUserSettings()
        {
        }
    }
}
