using OmniCore.Model.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Common.Data.Entities;

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
