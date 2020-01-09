using OmniCore.Model.Interfaces.Common.Data.Entities;
using OmniCore.Model.Interfaces.Common.Data.Repositories;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Repository.Sqlite.Entities;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class MigrationHistoryRepository : Repository<MigrationHistoryEntity, IMigrationHistoryEntity>, IMigrationHistoryRepository
    {
        public MigrationHistoryRepository(IRepositoryService repositoryService) : base(repositoryService)
        {
        }
    }
}