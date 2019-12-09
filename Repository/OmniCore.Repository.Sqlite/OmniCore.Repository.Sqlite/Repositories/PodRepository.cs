using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Repositories;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Repository.Sqlite.Entities;
using Unity;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class PodRepository : Repository<PodEntity, IPodEntity>, IPodRepository
    {
        public PodRepository(IRepositoryService repositoryService, IUnityContainer container) : base(repositoryService, container)
        {
        }

        public async IAsyncEnumerable<IPodEntity> ActivePods()
        {
            using var access = await RepositoryService.GetAccess(CancellationToken.None);
            var list = access.Connection.Table<PodEntity>().Where(e => !e.IsDeleted && e.State < PodState.Stopped);
            if (ExtendedAttributeProvider == null)
            {
                list = list.Where(e => e.ExtensionIdentifier == ExtendedAttributeProvider.Identifier);
            }
            foreach (var entity in await list.ToListAsync())
            {
                if (entity != null)
                    entity.ExtendedAttribute = ExtendedAttributeProvider?.New(entity.ExtensionValue);
                yield return entity;
            }
        }

        public async IAsyncEnumerable<IPodEntity> ArchivedPods()
        {
            using var access = await RepositoryService.GetAccess(CancellationToken.None);
            var list = access.Connection.Table<PodEntity>().Where(e => e.IsDeleted);
            if (ExtendedAttributeProvider == null)
            {
                list = list.Where(e => e.ExtensionIdentifier == ExtendedAttributeProvider.Identifier);
            }
            foreach (var entity in await list.ToListAsync())
            {
                if (entity != null)
                    entity.ExtendedAttribute = ExtendedAttributeProvider?.New(entity.ExtensionValue);
                yield return entity;
            }
        }
    }
}