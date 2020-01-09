using System.Collections.Generic;
using OmniCore.Model.Interfaces.Common.Data.Entities;

namespace OmniCore.Model.Interfaces.Common
{
    public interface IUser : IServerResolvable
    {
        IUserEntity Entity { get; }
        IAsyncEnumerable<ITherapySession> Sessions { get; }
    }
}