using System;
using OmniCore.Model.Interfaces.Common;
using SQLite;

namespace OmniCore.Model.Interfaces.Common.Data.Repositories
{
    public interface IRepositoryAccess : IDisposable, IServerResolvable
    {
        // todo: rewrite as either non-orm specific or IDbConnection
        SQLiteAsyncConnection Connection { get; }
    }
}