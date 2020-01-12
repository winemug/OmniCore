using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Platform.Common.Data.Entities
{
    public interface IPodResponseEntity : IEntity
    {
        IPodRequestEntity Request { get; set; }
        PodProgress? Progress { get; set; }
        bool? Faulted { get; set; }

        IPodResponseFaultAttributes FaultAttributes { get; set; }
        IPodResponseRadioAttributes RadioAttributes { get; set; }
        IPodResponseStatusAttributes StatusAttributes { get; set; }
        IPodResponseVersionAttributes VersionAttributes { get; set; }
    }
}
