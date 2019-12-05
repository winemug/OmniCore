using System;
using System.Data;
using System.Threading.Tasks;
using SQLite;

namespace OmniCore.Model.Interfaces.Repositories
{
    public interface IDataAccess : IDisposable
    {
        // todo: rewrite as either non-orm specific or IDbConnection
        SQLiteAsyncConnection Connection { get; }
    }
}