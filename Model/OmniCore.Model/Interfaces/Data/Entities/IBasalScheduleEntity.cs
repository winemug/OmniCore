namespace OmniCore.Model.Interfaces.Platform.Common.Data.Entities
{
    public interface IBasalScheduleEntity : IBasalScheduleAttributes, IEntity
    {
        ITherapyProfileEntity TherapyProfile { get; }
        IMedicationEntity Medication { get; }
    }
}
