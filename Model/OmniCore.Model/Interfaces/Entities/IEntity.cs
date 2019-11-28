using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface IEntity : IBasicEntity
    {
        DateTimeOffset? Updated { get; set; }
        bool Hidden { get; set; }
    }
}
