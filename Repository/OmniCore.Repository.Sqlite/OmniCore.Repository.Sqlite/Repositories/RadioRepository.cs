using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Repositories;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Repository.Sqlite.Entities;

namespace OmniCore.Repository.Sqlite.Repositories
{
    class RadioRepository : GenericRepository<RadioEntity, IRadioEntity>, IRadioRepository
    {
        public RadioRepository(IRepositoryService repositoryService) : base(repositoryService)
        {
        }

        public async Task<IRadioEntity> GetByProviderSpecificId(string providerSpecificId)
        {
            return await Connection.Table<RadioEntity>().FirstOrDefaultAsync(x => x.ProviderSpecificId == providerSpecificId);
        }
    }
}
