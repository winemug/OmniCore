namespace OmniCore.Model.Interfaces.Data.Entities
{
    public interface IBasalScheduleEntity : IBasalScheduleAttributes, IEntity
    {
        ITherapyProfileEntity TherapyProfile { get; }
        IMedicationEntity Medication { get; }
    }
}
