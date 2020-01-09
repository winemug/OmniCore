namespace OmniCore.Model.Interfaces.Common.Data.Entities
{
    public interface IBasalScheduleEntity : IBasalScheduleAttributes, IEntity
    {
        ITherapyProfileEntity TherapyProfile { get; }
        IMedicationEntity Medication { get; }
    }
}
