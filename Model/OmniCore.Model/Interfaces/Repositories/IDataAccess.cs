using System;
using System.Data;
using System.Threading.Tasks;
using SQLite;

namespace OmniCore.Model.Interfaces.Repositories
{
    public interface IDataAccess
    {
        // todo: rewrite as either non-orm specific or IDbConnection
        Task<SQLiteAsyncConnection> GetConnection();
    }
}