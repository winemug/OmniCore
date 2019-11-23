using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface IMedicationAttributes
    {
        string Name { get; set; }
        HormoneType Hormone { get; set; }
        string UnitName { get; set; }
        string UnitNameShort { get; set; }
        decimal UnitsPerMilliliter { get; set; }
        string ProfileCode { get; set; }
    }
}
