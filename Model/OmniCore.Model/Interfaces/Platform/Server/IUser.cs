using System.Collections.Generic;
using OmniCore.Model.Interfaces.Data.Entities;

namespace OmniCore.Model.Interfaces
{
    public interface IUser : IServerResolvable
    {
        IUserEntity Entity { get; }
        IAsyncEnumerable<ITherapySession> Sessions { get; }
    }
}