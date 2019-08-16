using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using SQLite;

namespace OmniCore.Mobile.Services
{
    public class PodRepository : IPodRepository
    {
        public readonly string DbPath;

        private readonly SQLiteAsyncConnection _connection;

        public PodRepository()
        {
            DbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "omnicore.db3");
            _connection = new SQLiteAsyncConnection(DbPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite
                                                                                  | SQLiteOpenFlags.FullMutex);
        }

        public async Task<IList<T>> GetActivePods<T>() where T : IPod, new()
        {
            await _connection.CreateTableAsync<T>();
            return await _connection.Table<T>()
                .Where(x => !x.Archived)
                .OrderByDescending(x => x.Created)
                .ToListAsync();
        }

        public async Task SavePod<T>(T pod) where T : IPod, new()
        {
            await _connection.CreateTableAsync<T>();
        }

        public void Dispose()
        {
            _connection.CloseAsync().ExecuteSynchronously();
        }
    }
}
