using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface IPodEntity : IPodAttributes, IReminderSettingsAttributes, IEntity
    {
        IUserEntity User { get; set; }
        IMedicationEntity Medication { get; set; }
        ITherapyProfileEntity TherapyProfile { get; set; }
        IList<IRadioEntity> Radios { get; set; }
    }
}
