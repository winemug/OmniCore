using OmniCore.Model.Interfaces.Platform;
using OmniCore.Repository.Sqlite.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Data.Entities;
using OmniCore.Model.Interfaces.Data.Repositories;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class MedicationRepository : Repository<MedicationEntity, IMedicationEntity>, IMedicationRepository
    {
        public MedicationRepository(IRepositoryService repositoryService) : base(repositoryService)
        {
        }
    }
}
