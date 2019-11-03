using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Entities
{
    public class Individual : UpdateableEntity
    {
        public string Name { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
    }
}
