using OmniCore.Model.Interfaces.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Sqlite.Entities
{
    public class Entity : BasicEntity, IEntity
    {
        public Entity() : base() {}

        public DateTimeOffset? Updated { get; set; }
        public bool Hidden { get; set; }
    }
}
