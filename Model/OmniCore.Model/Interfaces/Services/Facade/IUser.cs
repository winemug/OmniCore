using OmniCore.Model.Entities;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Interfaces.Services.Facade
{
    public interface IUser : IServiceInstance
    {
        UserEntity Entity { get; }
    }
}