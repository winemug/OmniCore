using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Data
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
