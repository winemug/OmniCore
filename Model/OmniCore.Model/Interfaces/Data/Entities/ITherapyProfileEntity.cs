namespace OmniCore.Model.Interfaces.Data.Entities
{
    public interface ITherapyProfileEntity : IEntity
    {
        string Name { get; set; }
        string Description { get; set; }
    }
}
