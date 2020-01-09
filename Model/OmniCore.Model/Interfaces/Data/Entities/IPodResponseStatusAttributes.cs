using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Common.Data.Entities
{
    public interface IPodResponseStatusAttributes
    {
        bool Faulted { get; set; }
        BasalState BasalState { get; set; }
        BolusState BolusState { get; set; }
        int NotDelivered { get; set; }
        int Delivered { get; set; }
        int Reservoir { get; set; }
        int MessageSequence { get; set; }
        int ActiveMinutes { get; set; }
        byte AlertMask { get; set; }
    }
}
