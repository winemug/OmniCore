using System;

namespace OmniCore.Model.Interfaces
{
    public interface IProfile
    {
        long? Id { get; set; }
        DateTimeOffset Created { get; set; }
        string Name { get; set; }
        decimal[] BasalSchedule { get; set; }
        int UtcOffset { get; set; }
    }
}