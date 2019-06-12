using OmniCore.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Data
{
    public interface IStatus
    {
        long? Id { get; set; }
        Guid PodId { get; set; }
        DateTime Created { get; set; }

        bool Faulted { get; set; }

        decimal NotDeliveredInsulin { get; set; }
        decimal DeliveredInsulin { get; set; }
        decimal Reservoir { get; set; }

        PodProgress Progress { get; set; }
        BasalState BasalState { get; set; }
        BolusState BolusState { get; set; }

        uint ActiveMinutes { get; set; }
        byte AlertMask { get; set; }
        int MessageSequence { get; set; }
    }
}
