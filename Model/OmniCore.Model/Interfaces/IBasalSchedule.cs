using System;

namespace OmniCore.Model.Interfaces
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
