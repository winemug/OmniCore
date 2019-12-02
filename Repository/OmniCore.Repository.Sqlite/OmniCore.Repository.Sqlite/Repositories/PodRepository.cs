using System.Collections.Generic;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Repositories;
using OmniCore.Repository.Sqlite.Entities;
using Unity;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class PodRepository : Repository<PodEntity, IPodEntity>, IPodRepository
    {
        public PodRepository(IDataAccess dataAccess, IUnityContainer container) : base(dataAccess, container)
        {
        }

        public async IAsyncEnumerable<IPodEntity> ActivePods()
        {
            var c = await GetConnection();
            var list = c.Table<PodEntity>().Where(e => !e.Hidden && e.State < PodState.Stopped);
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
            var c = await GetConnection();
            var list = c.Table<PodEntity>().Where(e => e.Hidden);
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