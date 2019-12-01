namespace OmniCore.Model.Interfaces.Entities
{
    public interface ITherapySessionEntity : IEntity
    {
        ITherapyProfileEntity Profile { get; set; }
    }
}