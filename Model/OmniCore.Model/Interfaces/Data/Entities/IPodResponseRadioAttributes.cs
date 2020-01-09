using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Common.Data.Entities
{
    public interface IPodResponseRadioAttributes
    {
        byte PodLowGain { get; set; }
        byte PodRssi { get; set; }

    }
}
