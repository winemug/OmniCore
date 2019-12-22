using System;
using SQLite;

namespace OmniCore.Model.Interfaces.Data.Repositories
{
    public interface IRepositoryAccess : IDisposable
    {
        // todo: rewrite as either non-orm specific or IDbConnection
        SQLiteAsyncConnection Connection { get; }
    }
}