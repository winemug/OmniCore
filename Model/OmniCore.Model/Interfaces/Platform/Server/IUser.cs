using System.Collections.Generic;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface IUser : IServerResolvable
    {
        IUserEntity Entity { get; }
        IAsyncEnumerable<ITherapySession> Sessions { get; }
    }
}