using System;
using System.Collections.Generic;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface IBasalScheduleAttributes
    {
        IList<(TimeSpan start, decimal hourlyRate)> Schedule { get; set; }
    }
}
