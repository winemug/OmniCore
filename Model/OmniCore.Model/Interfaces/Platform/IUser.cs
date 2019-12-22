using System.Collections.Generic;
using OmniCore.Model.Interfaces.Data.Entities;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IUser
    {
        IUserEntity Entity { get; }
        IAsyncEnumerable<ITherapySession> Sessions { get; }
    }
}