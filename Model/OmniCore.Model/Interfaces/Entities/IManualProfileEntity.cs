using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface IManualProfileEntity : IUserProfileAttributes, IEntity
    {
        ITherapyProfileEntity Therapy { get; set; }
        IList<IBasalScheduleEntity> BasalSchedules { get; set; }
    }
}
