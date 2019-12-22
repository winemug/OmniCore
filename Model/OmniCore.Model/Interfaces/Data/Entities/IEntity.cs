using System;

namespace OmniCore.Model.Interfaces.Data.Entities
{
    public interface IEntity
    {
        long Id { get; set; }
        DateTimeOffset Created { get; set; }
        DateTimeOffset? Updated { get; set; }
        bool IsDeleted { get; set; }
    }
}
