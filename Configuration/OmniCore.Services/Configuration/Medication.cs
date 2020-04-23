using OmniCore.Model.Entities;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Services.Configuration
{
    public class Medication : IMedication
    {
        public MedicationEntity Entity { get; set; }
    }
}