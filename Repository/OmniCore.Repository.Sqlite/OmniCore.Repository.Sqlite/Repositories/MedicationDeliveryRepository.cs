using OmniCore.Model.Interfaces.Data.Repositories;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using OmniCore.Repository.Sqlite.Entities;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class MedicationDeliveryRepository : Repository<MedicationDeliveryEntity, IMedicationDeliveryEntity>, IMedicationDeliveryRepository
    {
        public MedicationDeliveryRepository(IRepositoryService repositoryService) : base(repositoryService)
        {
        }
    }
}