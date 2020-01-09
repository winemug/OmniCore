using System;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common.Data.Entities;

namespace OmniCore.Model.Interfaces.Common.Data.Repositories
{
    public interface IRadioRepository : IRepository<IRadioEntity>
    {
        Task<IRadioEntity> ByDeviceUuid(Guid deviceUuid);
    }
}
