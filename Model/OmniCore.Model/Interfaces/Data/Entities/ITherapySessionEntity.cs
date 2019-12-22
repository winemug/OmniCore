namespace OmniCore.Model.Interfaces.Data.Entities
{
    public interface ITherapySessionEntity : IEntity
    {
        ITherapyProfileEntity Profile { get; set; }
    }
}