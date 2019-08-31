using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Mobile.Services
{
    public class SqliteRepository : IRepository
    {
        private SQLiteAsyncConnection _connection;

        public readonly string DbPath;

        public async Task<SQLiteAsyncConnection> GetConnection()
        {
            if (_connection == null)
            {
                await Initialize();
            }
            return _connection;
        }

        public SqliteRepository()
        {
            DbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "omnicore.db3");
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.CloseAsync().ExecuteSynchronously();
                _connection = null;
            }
        }

        public async Task Initialize()
        {
            if (_connection != null)
            {
                _connection = new SQLiteAsyncConnection(DbPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite
                                                                                      | SQLiteOpenFlags.FullMutex);
            }
        }
    }
}
