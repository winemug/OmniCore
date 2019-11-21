using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Attributes;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface IPodEntity : IPodAttributes, IReminderSettingsAttributes, IEntity
    {
        IUserEntity User { get; }
        IMedicationEntity Medication { get; }
        IList<IRadioEntity> Radios { get; }
    }
}
