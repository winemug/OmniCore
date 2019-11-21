using OmniCore.Model.Interfaces.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Sqlite.Entities
{
    public class Entity : IEntity
    {
        [AutoIncrement, PrimaryKey]
        public long Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public bool Hidden { get; set; }
    }
}
