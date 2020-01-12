using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Repository.Sqlite.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform.Common.Data;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using OmniCore.Model.Interfaces.Platform.Common.Data.Repositories;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class MedicationRepository : Repository<MedicationEntity, IMedicationEntity>, IMedicationRepository
    {
        public MedicationRepository(IRepositoryService repositoryService) : base(repositoryService)
        {
        }
    }
}
