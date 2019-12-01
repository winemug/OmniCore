using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface IBasalScheduleEntity : IBasalScheduleAttributes, IEntity
    {
        ITherapyProfileEntity TherapyProfile { get; }
        IMedicationEntity Medication { get; }
    }
}
