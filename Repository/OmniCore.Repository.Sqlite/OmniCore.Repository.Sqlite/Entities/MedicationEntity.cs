using OmniCore.Model.Enumerations;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Common.Data.Entities;

namespace OmniCore.Repository.Sqlite.Entities
{
    [Table("Medication")]
    public class MedicationEntity : Entity, IMedicationEntity
    {
        public string Name { get; set; }
        public HormoneType Hormone { get; set; }
        public string UnitName { get; set; }
        public string UnitNameShort { get; set; }
        public decimal UnitsPerMilliliter { get; set; }
        public string ProfileCode { get; set; }
    }
}
