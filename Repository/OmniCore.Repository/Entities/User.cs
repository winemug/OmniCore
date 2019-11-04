using OmniCore.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Entities
{
    public class User : UpdateableEntity
    {
        public string Name { get; set; }
        public bool ManagedRemotely { get; set; }
        public Genotype? Gender { get; set; }
        public DateTimeOffset? DateOfBirth { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }
        public PhysicalActivityLevel? PhysicalActivity { get; set; }
    }
}
