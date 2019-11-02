using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Entities
{
    public class Entity
    {
        [AutoIncrement, PrimaryKey]
        public long? Id { get; set; }
        public DateTimeOffset Created {get; set;}
    }

    public class UpdateableEntity : Entity
    {
        public DateTimeOffset Updated { get; set; }
    }
}
