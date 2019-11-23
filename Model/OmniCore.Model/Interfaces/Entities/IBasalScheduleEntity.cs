using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface IBasalScheduleEntity : IBasalScheduleAttributes, IEntity
    {
        IManualProfileEntity UserProfile { get; }
        IMedicationEntity Medication { get; }
    }
}
