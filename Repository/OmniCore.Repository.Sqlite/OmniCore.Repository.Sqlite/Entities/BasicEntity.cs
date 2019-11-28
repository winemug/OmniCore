using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Entities;
using SQLite;

namespace OmniCore.Repository.Sqlite.Entities
{
    public class BasicEntity : IBasicEntity
    {
        public BasicEntity()
        {
            Created = DateTimeOffset.UtcNow;
        }

        [AutoIncrement, PrimaryKey]
        public long Id { get; set; }

        public DateTimeOffset Created { get; set; }
    }
}
