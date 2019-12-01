using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Repositories;
using OmniCore.Repository.Sqlite.Entities;
using Unity;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class PodRepository : Repository<PodEntity, IPodEntity>
    {
        public PodRepository(IDataAccess dataAccess, IUnityContainer container) : base(dataAccess, container)
        {
        }
    }
}