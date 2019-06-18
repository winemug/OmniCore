using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Data
{
    public interface IProfile
    {
        long? Id { get; set; }
        DateTimeOffset Created { get; set; }

        decimal[] BasalSchedule { get; set; }
        int UtcOffset { get; set; }
    }
}
