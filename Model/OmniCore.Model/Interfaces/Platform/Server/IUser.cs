using System.Collections.Generic;
using OmniCore.Model.Entities;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface IUser : IServerResolvable
    {
        UserEntity Entity { get; }
    }
}