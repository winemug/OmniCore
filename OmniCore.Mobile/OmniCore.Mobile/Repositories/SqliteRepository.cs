using OmniCore.Impl.Eros;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Mobile.Repositories
{
    public class SqliteRepository<T> : IRepository<T> where T : IEntity, new()
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
                await MigrateRepository(_connection);
            }
        }

        protected virtual async Task MigrateRepository(SQLiteAsyncConnection connection)
        {
            await connection.CreateTableAsync<T>();
        }

        public async Task<T> CreateOrUpdate(T entity)
        {
            await Initialize();
            var dt = DateTimeOffset.UtcNow;
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
                entity.Created = dt;
            }
            entity.Updated = dt;
            await _connection.InsertOrReplaceAsync(entity);
            return entity;
        }

        public async Task<T> Read(Guid entityId)
        {
            await Initialize();
            return await _connection.Table<T>().FirstOrDefaultAsync(t => t.Id == entityId);
        }

        public async Task Delete(Guid entityId)
        {
            await Initialize();
            await _connection.DeleteAsync<T>(entityId);
        }
    }
}
