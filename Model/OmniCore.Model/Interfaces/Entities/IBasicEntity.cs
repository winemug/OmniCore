using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface IBasicEntity // ya basic
    {
        long Id { get; set; }
        DateTimeOffset Created { get; set; }
    }
}
