using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Data.Entities;

namespace OmniCore.Repository.Sqlite.Entities
{
    public class Entity : IEntity
    {
        public Entity()
        {
            Created = DateTimeOffset.UtcNow;
        }

        [AutoIncrement, PrimaryKey]
        public long Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public bool IsDeleted { get; set; }
    }
}
