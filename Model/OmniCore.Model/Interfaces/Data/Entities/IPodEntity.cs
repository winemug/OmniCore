using System.Collections.Generic;

namespace OmniCore.Model.Interfaces.Platform.Common.Data.Entities
{
    public interface IPodEntity : IPodAttributes, IReminderSettingsAttributes, IEntity
    {
        IUserEntity User { get; set; }
        IMedicationEntity Medication { get; set; }
        ITherapyProfileEntity TherapyProfile { get; set; }
        IList<IRadioEntity> Radios { get; set; }
    }
}
