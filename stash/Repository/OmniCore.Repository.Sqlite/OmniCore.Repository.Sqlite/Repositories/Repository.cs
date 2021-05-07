using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Repository.Sqlite.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Data.Repositories;
using OmniCore.Model.Interfaces.Platform.Common.Data;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using OmniCore.Model.Interfaces.Platform.Common.Data.Repositories;
using SQLite;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class Repository<ConcreteType, InterfaceType> : IRepository<InterfaceType>
        where InterfaceType : IEntity
        where ConcreteType : Entity, InterfaceType, new()
    {
        protected IRepositoryAccessProvider AccessProvider;
        protected IRepositoryAccess DirectAccess = null;
        
        public Repository(IRepositoryService repositoryService)
        {
            AccessProvider = repositoryService.AccessProvider;
        }
        
        public virtual InterfaceType New()
        {
            return new ConcreteType()
            {
                SyncId = Guid.NewGuid()
            };
        }
        public virtual Task EnsureSchemaAndDefaults(CancellationToken cancellationToken)
        {
            return SchemaTask(c => c.CreateTableAsync<ConcreteType>(), cancellationToken);
        }

        public IRepository<InterfaceType> WithAccessProvider(IRepositoryAccessProvider accessProvider)
        {
            AccessProvider = accessProvider;
            return this;
        }

        public IRepository<InterfaceType> WithDirectAccess(IRepositoryAccess access)
        {
            DirectAccess = access;
            return this;
        }

        protected async Task DataTask(Func<SQLiteAsyncConnection, Task> dataTask, CancellationToken cancellationToken)
        {
            if (DirectAccess == null)
            {
                try
                {
                    DirectAccess = await AccessProvider.ForData(cancellationToken);
                    await dataTask(DirectAccess.Connection);
                }
                catch
                {
                    DirectAccess?.Dispose();
                    throw;
                }
            }
            else
                await dataTask(DirectAccess.Connection);
        }
        
        protected async Task<T> DataTask<T>(Func<SQLiteAsyncConnection, Task<T>> dataTask, CancellationToken cancellationToken)
        {
            if (DirectAccess == null)
            {
                try
                {
                    DirectAccess = await AccessProvider.ForData(cancellationToken);
                    return await dataTask(DirectAccess.Connection);
                }
                catch
                {
                    DirectAccess?.Dispose();
                    throw;
                }
            }
            else
                return await dataTask(DirectAccess.Connection);
        }

        protected async Task SchemaTask(Func<SQLiteAsyncConnection, Task> schemaTask, CancellationToken cancellationToken)
        {
            if (DirectAccess == null)
            {
                try
                {
                    DirectAccess = await AccessProvider.ForSchema(cancellationToken);
                    await schemaTask(DirectAccess.Connection);
                }
                catch
                {
                    DirectAccess?.Dispose();
                    throw;
                }
            }
            else
                await schemaTask(DirectAccess.Connection);
        }
        
        protected async Task<T> SchemaTask<T>(Func<SQLiteAsyncConnection, Task<T>> schemaTask, CancellationToken cancellationToken)
        {
            if (DirectAccess == null)
            {
                try
                {
                    DirectAccess = await AccessProvider.ForSchema(cancellationToken);
                    return await schemaTask(DirectAccess.Connection);
                }
                catch
                {
                    DirectAccess?.Dispose();
                    throw;
                }
            }
            else
                return await schemaTask(DirectAccess.Connection);
        }
        
        public virtual Task Delete(InterfaceType entity, CancellationToken cancellationToken)
        {
            return DataTask(c => c.DeleteAsync(entity), cancellationToken);
        }

        public virtual async Task Update(InterfaceType entity, CancellationToken cancellationToken)
        {
            await DataTask(c =>
            {
                entity.Updated = DateTimeOffset.UtcNow;

                if (!entity.SyncId.HasValue)
                    entity.SyncId = Guid.NewGuid();

                return c.UpdateAsync(entity);
            }, cancellationToken);
        }

        public virtual async Task Create(InterfaceType entity, CancellationToken cancellationToken)
        {
            await DataTask(c =>
            {
                entity.Created = DateTimeOffset.UtcNow;

                if (!entity.SyncId.HasValue)
                    entity.SyncId = Guid.NewGuid();

                return c.InsertAsync(entity, typeof(ConcreteType));
            }, cancellationToken);
        }
        public virtual async Task<InterfaceType> Read(long id, CancellationToken cancellationToken)
        {
            return await DataTask(c =>
            {
                return c.Table<ConcreteType>()
                    .FirstOrDefaultAsync(x => x.Id == id);
            }, cancellationToken);
        }

        public async Task<IList<InterfaceType>> All(CancellationToken cancellationToken)
        {
            return (await DataTask(c =>
            {
                return c.Table<ConcreteType>().Where(e => !e.IsDeleted)
                    .ToListAsync();
            }, cancellationToken)).ToList<InterfaceType>();
        }
    }
}
