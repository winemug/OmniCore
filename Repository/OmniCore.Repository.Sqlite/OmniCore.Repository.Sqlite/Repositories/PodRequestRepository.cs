using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using OmniCore.Model.Interfaces.Platform.Common.Data.Repositories;
using OmniCore.Repository.Sqlite.Entities;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class PodRequestRepository : Repository<PodRequestEntity, IPodRequestEntity>, IPodRequestRepository
    {
        public PodRequestRepository(IRepositoryService repositoryService) : base(repositoryService)
        {
        }
    }
}