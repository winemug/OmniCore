using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Sqlite.Entities
{
    public class UserEntity : Entity, IUserEntity
    {
        public bool ManagedRemotely { get; set; }
        public string Name { get; set; }
        public Genotype? Gender { get; set; }
        public DateTimeOffset? DateOfBirth { get; set; }
    }
}
