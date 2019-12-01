using OmniCore.Model.Interfaces.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

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
        public bool Hidden { get; set; }

        [Ignore]
        public IExtendedAttribute ExtendedAttribute { get; set; }
        
        public string ExtensionIdentifier
        {
            get => ExtendedAttribute?.ExtensionIdentifier;
            set { }
        }

        public string ExtensionValue
        {
            get => ExtendedAttribute?.ExtensionValue;
            set
            {
                if (ExtendedAttribute != null)
                    ExtendedAttribute.ExtensionValue = value;
            }
        }
    }
}
