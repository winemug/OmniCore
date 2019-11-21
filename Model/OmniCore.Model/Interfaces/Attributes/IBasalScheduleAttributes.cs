using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OmniCore.Model.Interfaces.Attributes
{
    public interface IBasalScheduleAttributes
    {
        IList<(TimeSpan start, decimal hourlyRate)> Schedule { get; set; }
    }
}
