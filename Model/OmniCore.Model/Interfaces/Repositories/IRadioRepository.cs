using OmniCore.Model.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Repositories
{
    public interface IRadioRepository : IGenericRepository<IRadioEntity>
    {
        Task<IRadioEntity> GetByProviderSpecificId(string providerSpecificId);
    }
}
