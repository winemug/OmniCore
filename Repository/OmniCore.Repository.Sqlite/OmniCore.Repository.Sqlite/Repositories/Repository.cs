using OmniCore.Model.Interfaces.Common;
using OmniCore.Repository.Sqlite.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common.Data;
using OmniCore.Model.Interfaces.Common.Data.Entities;
using OmniCore.Model.Interfaces.Common.Data.Repositories;
using SQLite;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class Repository<ConcreteType, InterfaceType> : IRepository<InterfaceType>
        where InterfaceType : IEntity
        where ConcreteType : Entity, InterfaceType, new()
    {
        protected readonly IRepositoryService RepositoryService;

        public Repository(IRepositoryService repositoryService)
        {
            RepositoryService = repositoryService;
        }

        public InterfaceType New()
        {
            return new ConcreteType()
            {
                Uuid = Guid.NewGuid()
            };
        }
        public async Task Delete(InterfaceType entity, CancellationToken cancellationToken)
        {
            using var access = await RepositoryService.GetAccess(cancellationToken);
            await access.Connection.DeleteAsync(entity);
        }

        public virtual async Task Update(InterfaceType entity, CancellationToken cancellationToken)
        {
            using var access = await RepositoryService.GetAccess(cancellationToken);
            entity.Updated = DateTimeOffset.UtcNow;

            if (!entity.Uuid.HasValue)
                entity.Uuid = Guid.NewGuid();

            await access.Connection.UpdateAsync(entity);
        }

        public virtual async Task Create(InterfaceType entity, CancellationToken cancellationToken)
        {
            using var access = await RepositoryService.GetAccess(cancellationToken);
            entity.Created = DateTimeOffset.UtcNow;

            if (!entity.Uuid.HasValue)
                entity.Uuid = Guid.NewGuid();
            
            await access.Connection.InsertAsync(entity, typeof(ConcreteType));
        }
        public virtual async Task<InterfaceType> Read(long id, CancellationToken cancellationToken)
        {
            using var access = await RepositoryService.GetAccess(cancellationToken);
            var entity = await access.Connection.Table<ConcreteType>().FirstOrDefaultAsync(x => x.Id == id);
            return entity;
        }

        public virtual async IAsyncEnumerable<InterfaceType> All
            ([EnumeratorCancellation] CancellationToken cancellationToken)
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
