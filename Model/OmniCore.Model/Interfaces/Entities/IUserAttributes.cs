using OmniCore.Model.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface IUserAttributes
    {
        string Name { get; set; }
        Genotype? Gender { get; set; }
        DateTimeOffset? DateOfBirth { get; set; }
    }
}
