using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IEntity
    {
        Guid Id  {get; set;}
        DateTimeOffset Created {get; set;}
        DateTimeOffset Updated {get; set;}
    }
}
