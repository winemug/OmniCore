using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface ITherapyProfileEntity : IEntity
    {
        string Name { get; set; }
        string Description { get; set; }
    }
}
