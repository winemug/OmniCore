using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Entities;
using OmniCore.Model.Interfaces.Services.Facade;

namespace OmniCore.Services.Configuration
{
    public class Medication : IMedication
    {
        public MedicationEntity Entity { get; set; }
    }
}
