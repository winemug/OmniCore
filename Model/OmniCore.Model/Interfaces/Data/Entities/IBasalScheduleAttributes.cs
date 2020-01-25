using System;
using System.Collections.Generic;

namespace OmniCore.Model.Interfaces.Platform.Common.Data.Entities
{
    public interface IBasalScheduleAttributes
    {
        IList<(TimeSpan start, decimal hourlyRate)> BasalScheduleEntries { get; set; }
    }
}
