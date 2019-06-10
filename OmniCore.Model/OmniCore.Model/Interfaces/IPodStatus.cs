using OmniCore.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IPodStatus
    {
        long? Id { get; set; }
        long ResultId { get; set; }


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
