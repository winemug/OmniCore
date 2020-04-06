using System;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Model.Entities
{
    public class Entity : IEntity
    {
        public long Id { get; set; }
        public Guid? SyncId { get; set; } = Guid.NewGuid();
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime? Updated { get; set; }
        public bool IsDeleted { get; set; }
    }
}