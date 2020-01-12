namespace OmniCore.Model.Interfaces.Platform.Common.Data.Entities
{
    public interface IUserEntity : IUserAttributes, IEntity
    {
        bool ManagedRemotely { get; set; }
    }
}
