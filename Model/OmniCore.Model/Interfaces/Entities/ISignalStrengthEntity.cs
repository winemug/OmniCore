using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface ISignalStrengthEntity : ISignalStrengthAttributes, IEntity
    {
        IPodEntity Pod { get; set; }
        IRadioEntity Radio { get; set; }
        IPodRequestEntity Request { get; set; }
    }
}
