using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IPodRadioIndicators
    {
        long? Id { get; set; }
        Guid PodId { get; set; }
        DateTime Created { get; set; }

        int? RadioLowGain { get; set; }
        int? RadioRssi { get; set; }
    }
}
