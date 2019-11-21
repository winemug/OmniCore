using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Repositories;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Repository.Sqlite.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class MedicationRepository : GenericRepository<MedicationEntity, IMedicationEntity>, IMedicationRepository
    {
        public MedicationRepository(IRepositoryService repositoryService) : base(repositoryService)
        {
        }
    }
}
