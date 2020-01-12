namespace OmniCore.Model.Interfaces.Platform.Common.Data.Entities
{
    public interface ITherapySessionEntity : IEntity
    {
        ITherapyProfileEntity Profile { get; set; }
    }
}