using System;

namespace OmniCore.Model.Interfaces.Common.Data.Entities
{
    public interface IEntity
    {
        long Id { get; set; }
        Guid? Uuid { get; set; }
        DateTimeOffset Created { get; set; }
        DateTimeOffset? Updated { get; set; }
        bool IsDeleted { get; set; }
    }
}
