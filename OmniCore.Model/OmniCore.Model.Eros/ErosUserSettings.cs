using System;
using OmniCore.Model.Interfaces;
using SQLite;

namespace OmniCore.Model.Eros
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
