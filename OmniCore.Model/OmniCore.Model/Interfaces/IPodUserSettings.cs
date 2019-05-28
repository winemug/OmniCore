using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IPodUserSettings
    {
        uint? Id { get; set; }
        DateTime Created { get; set; }
        Guid PodId { get; set; }

        decimal? ReservoirWarningAtLevel { get; set; }
        int? ExpiryWarningAtMinute { get; set; }
    }
}
