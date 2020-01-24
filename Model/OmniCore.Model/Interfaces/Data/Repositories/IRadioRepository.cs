using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;

namespace OmniCore.Model.Interfaces.Platform.Common.Data.Repositories
{
    public interface IRadioRepository : IRepository<IRadioEntity>
    {
        Task<IRadioEntity> ByDeviceUuid(Guid deviceUuid, CancellationToken cancellationToken);
    }
}
