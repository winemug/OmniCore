using System;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Data.Entities;

namespace OmniCore.Model.Interfaces.Data.Repositories
{
    public interface IRadioRepository : IRepository<IRadioEntity>
    {
        Task<IRadioEntity> ByDeviceUuid(Guid deviceUuid);
    }
}
