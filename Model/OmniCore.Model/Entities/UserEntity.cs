using System;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Entities
{
    public class UserEntity : Entity
    {
        public bool ManagedRemotely { get; set; }
        public string Name { get; set; }
        public Genotype? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }

}