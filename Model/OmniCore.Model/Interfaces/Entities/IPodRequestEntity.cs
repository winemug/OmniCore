using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface IPodRequestEntity : IPodRequestAttributes, IEntity
    {
        IPodEntity Pod { get; }
    }
}
