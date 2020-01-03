using System;
using OmniCore.Model.Interfaces.Platform;
using SQLite;

namespace OmniCore.Model.Interfaces.Data.Repositories
{
    public interface IRepositoryAccess : IDisposable, IServerResolvable
    {
        // todo: rewrite as either non-orm specific or IDbConnection
        SQLiteAsyncConnection Connection { get; }
    }
}