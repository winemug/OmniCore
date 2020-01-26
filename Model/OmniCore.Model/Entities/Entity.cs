using System;

namespace OmniCore.Model.Entities
{
    public class Entity
    {
        public long Id { get; set; }
        public Guid? SyncId { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public bool IsDeleted { get; set; }
    }
}