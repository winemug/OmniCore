namespace OmniCore.Model.Interfaces.Data.Entities
{
    public interface IUserEntity : IUserAttributes, IEntity
    {
        bool ManagedRemotely { get; set; }
    }
}
