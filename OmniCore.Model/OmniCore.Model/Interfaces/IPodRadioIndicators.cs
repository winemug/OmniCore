using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IPodRadioIndicators
    {
        uint? Id { get; set; }
        DateTime Created { get; set; }
        Guid PodId { get; set; }

        int? RadioLowGain { get; set; }
        int? RadioRssi { get; set; }
    }
}
