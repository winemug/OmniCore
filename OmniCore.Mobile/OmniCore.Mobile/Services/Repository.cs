using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using SQLite;

namespace OmniCore.Mobile.Services
{
    public class Repository : IRepository
    {
        public readonly string DbPath;

        private readonly SQLiteAsyncConnection _connection;

        private Repository()
        {
            DbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "omnicore.db3");
            _connection = new SQLiteAsyncConnection(DbPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite
                                                                                  | SQLiteOpenFlags.FullMutex);
        }

        private static Repository _instance = null;

        public static Repository Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Repository();
                return _instance;
            }
        }

        public async Task<IList<T>> GetActivePods<T>() where T : IPod, new()
        {
            await _connection.CreateTableAsync<T>();
            return await _connection.Table<T>()
                .Where(x => !x.Archived)
                .OrderByDescending(x => x.Created)
                .ToListAsync();
        }

        public async Task SavePod<T>(IPod pod) where T : IPod, new()
        {
            await _connection.CreateTableAsync<T>();
        }

        public void Dispose()
        {
            _connection.CloseAsync().ExecuteSynchronously();
            _instance = null;
        }
    }
}
