using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface IEntity
    {
        long Id { get; set; }
        DateTimeOffset Created { get; set; }
        DateTimeOffset? Updated { get; set; }
        bool IsDeleted { get; set; }
        IExtendedAttribute ExtendedAttribute { get; set; }
    }
}
