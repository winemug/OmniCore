using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Repositories;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Repository.Sqlite.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SQLite;
using Unity;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class Repository<ConcreteType, InterfaceType> : IRepository<InterfaceType>
        where InterfaceType : IEntity
        where ConcreteType : Entity, InterfaceType, new()
    {
        protected readonly IRepositoryService RepositoryService;
        private readonly IUnityContainer Container;

        public Repository(IRepositoryService repositoryService, IUnityContainer container)
        {
            RepositoryService = repositoryService;
            Container = container;
        }

        public InterfaceType New()
        {
            return new ConcreteType();
        }
        public async Task Delete(InterfaceType entity, CancellationToken cancellationToken)
        {
            using var access = await RepositoryService.GetAccess(cancellationToken);
            await access.Connection.DeleteAsync(entity);
        }

        public virtual async Task Initialize(Version migrateFrom, SQLiteAsyncConnection connection, CancellationToken cancellationToken)
        {
            if (migrateFrom == null)
            {
                await connection.CreateTableAsync<ConcreteType>();
            }
        }

        public virtual async Task Update(InterfaceType entity, CancellationToken cancellationToken)
        {
            using var access = await RepositoryService.GetAccess(cancellationToken);
            await access.Connection.UpdateAsync(entity);
        }

        public virtual async Task Create(InterfaceType entity, CancellationToken cancellationToken)
        {
            using var access = await RepositoryService.GetAccess(cancellationToken);
            await access.Connection.InsertAsync(entity, typeof(ConcreteType));
        }
        public virtual async Task<InterfaceType> Read(long id, CancellationToken cancellationToken)
        {
            using var access = await RepositoryService.GetAccess(cancellationToken);
            var entity = await access.Connection.Table<ConcreteType>().FirstOrDefaultAsync(x => x.Id == id);
            return entity;
        }

        public virtual async IAsyncEnumerable<InterfaceType> All(CancellationToken cancellationToken)
        {
            using var access = await RepositoryService.GetAccess(cancellationToken);
            var list = access.Connection.Table<ConcreteType>().Where(e => !e.IsDeleted);

            foreach (var entity in await list.ToListAsync())
            {
                yield return entity;
            }
        }
    }
}
