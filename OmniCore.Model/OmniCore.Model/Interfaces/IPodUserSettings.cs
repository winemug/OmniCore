using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IPodUserSettings
    {
        long? Id { get; set; }
        Guid PodId { get; set; }
        DateTime Created { get; set; }

        decimal? ReservoirWarningAtLevel { get; set; }
        int? ExpiryWarningAtMinute { get; set; }
    }
}
