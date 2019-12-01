using OmniCore.Model.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Repositories;
using SQLite;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class DataAccess : IDataAccess
    {
        private SQLiteAsyncConnection ConnectionInternal;
        public DataAccess(IRepositoryService repositoryService)
        {
            ConnectionInternal = new SQLiteAsyncConnection
                (repositoryService.RepositoryPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
        }
        public async Task<SQLiteAsyncConnection> GetConnection()
        {
            return ConnectionInternal;
        }
    }
}
