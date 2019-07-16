using System;

namespace OmniCore.Model.Interfaces
{
    public interface IUserSettings
    {
        long? Id { get; set; }
        Guid PodId { get; set; }
        DateTimeOffset Created { get; set; }

        decimal? ReservoirWarningAtLevel { get; set; }
        int? ExpiryWarningAtMinute { get; set; }
    }
}
