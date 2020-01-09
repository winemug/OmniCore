namespace OmniCore.Model.Interfaces.Common.Data.Entities
{
    public interface IUserEntity : IUserAttributes, IEntity
    {
        bool ManagedRemotely { get; set; }
    }
}
