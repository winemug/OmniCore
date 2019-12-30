using System.Collections.Generic;
using OmniCore.Model.Interfaces.Data.Entities;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IUser
    {
        IUserEntity Entity { get; }
        IAsyncEnumerable<ITherapySession> Sessions { get; }
    }
}