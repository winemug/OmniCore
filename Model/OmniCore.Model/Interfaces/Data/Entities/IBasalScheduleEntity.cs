namespace OmniCore.Model.Interfaces.Platform.Common.Data.Entities
{
    public interface IBasalScheduleEntity : IBasalScheduleAttributes, IEntity
    {
        IUserEntity User { get; }
        ITherapyProfileEntity TherapyProfile { get; }
        IMedicationEntity Medication { get; }
    }
}
