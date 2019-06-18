using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Data
{
    public interface IBasalSchedule
    {
        long? Id { get; set; }
        Guid PodId { get; set; }
        DateTimeOffset Created { get; set; }

        int UtcOffset { get; set; }
        decimal[] BasalSchedule { get; set; }
        DateTimeOffset PodDateTime { get; set; }
    }
}
