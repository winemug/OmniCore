using System;
using SQLite;

namespace OmniCore.Model.Interfaces.Platform.Common.Data.Repositories
{
    public interface IRepositoryAccess : IDisposable, IServerResolvable
    {
        SQLiteAsyncConnection Connection { get; }
    }
}