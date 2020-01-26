using System.Collections.Generic;

namespace OmniCore.Model.Interfaces.Platform.Common.Data.Entities
{
    public interface IPodEntity : IPodAttributes, IPodSettingsAttributes, IBasalScheduleAttributes, IEntity
    {
        IUserEntity User { get; set; }
        IMedicationEntity Medication { get; set; }
        ITherapyProfileEntity TherapyProfile { get; set; }
        IBasalScheduleEntity ReferenceBasalSchedule { get; set; }
        IRadioEntity Radio { get; set; }
    }
}
