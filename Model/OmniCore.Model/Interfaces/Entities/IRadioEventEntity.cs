using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface IRadioEventEntity : IRadioEventAttributes, IEntity
    {
        IRadioEntity Radio { get; set; }
        IPodEntity Pod { get; set; }
        IPodRequestEntity Request { get; set; }
    }
}
