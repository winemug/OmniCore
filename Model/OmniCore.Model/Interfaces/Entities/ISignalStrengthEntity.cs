using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface ISignalStrengthEntity : ISignalStrengthAttributes, IBasicEntity
    {
        IPodEntity Pod { get; set; }
        IRadioEntity Radio { get; set; }
        IPodRequestEntity Request { get; set; }
    }
}
