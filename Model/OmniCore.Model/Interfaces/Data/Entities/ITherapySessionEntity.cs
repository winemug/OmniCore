namespace OmniCore.Model.Interfaces.Common.Data.Entities
{
    public interface ITherapySessionEntity : IEntity
    {
        ITherapyProfileEntity Profile { get; set; }
    }
}