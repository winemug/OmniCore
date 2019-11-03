using OmniCore.Repository.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Repository
{
    public class SqliteRepositoryWithUpdate<T> : SqliteRepository<T> where T : UpdateableEntity, new()
    {
        public virtual async Task<T> CreateOrUpdate(T entity)
        {
            var c = await GetConnection();
            if (!entity.Id.HasValue)
            {
                entity.Created = DateTimeOffset.UtcNow;
                await c.InsertAsync(entity);
            }
            else
            {
                entity.Updated = DateTimeOffset.UtcNow;
                await c.UpdateAsync(entity);
            }
            return entity;
        }
    }

    public class SqliteRepository<T> : IDisposable where T : Entity, new()
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
            try
            {
                _connection?.CloseAsync().Wait();
            } catch { }
            _connection = null;
        }

        public async Task Initialize()
        {
            if (_connection == null)
            {
                _connection = new SQLiteAsyncConnection(DbPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
                await MigrateRepository(_connection);
            }

        }

        protected virtual async Task MigrateRepository(SQLiteAsyncConnection connection)
        {
            await connection.CreateTableAsync<T>();
        }

        public virtual async Task<T> Create(T entity)
        {
            var c = await GetConnection();
            if (!entity.Id.HasValue)
            {
                entity.Created = DateTimeOffset.UtcNow;
                await c.InsertAsync(entity);
            }
            return entity;
        }

        public virtual async Task<T> Read(long entityId)
        {
            var c = await GetConnection();
            return await c.Table<T>().FirstOrDefaultAsync(t => t.Id == entityId);
        }

        public virtual async Task<AsyncTableQuery<T>> ForQuery()
        {
            var c = await GetConnection();
            return c.Table<T>();
        }

        public virtual async Task Delete(long entityId)
        {
            var c = await GetConnection();
            await c.DeleteAsync<T>(entityId);
        }
    }
}
