using OmniCore.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Entities
{
    public class Medication : UpdateableEntity
    {
        public string Name { get; set; }
        public HormoneType Hormone { get; set; }

        public string UnitName { get; set; }
        public string UnitNameShort { get; set; }
        public decimal UnitsPerMilliliter { get; set; }
    }
}
