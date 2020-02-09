using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Entities
{
    public class MedicationEntity : Entity
    {
        public string Name { get; set; }
        public HormoneType Hormone { get; set; }
        public string UnitName { get; set; }
        public string UnitNameShort { get; set; }
        public decimal UnitsPerMilliliter { get; set; }
        public string ProfileCode { get; set; }
    }
}