using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IPodRadioIndicators
    {
        long? Id { get; set; }
        long ResultId { get; set; }

        int? RadioLowGain { get; set; }
        int? RadioRssi { get; set; }
    }
}
