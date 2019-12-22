using System;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Data.Entities
{
    public interface IUserAttributes
    {
        string Name { get; set; }
        Genotype? Gender { get; set; }
        DateTimeOffset? DateOfBirth { get; set; }
    }
}
