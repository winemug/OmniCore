using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Repositories;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Repository.Sqlite.Entities;
using Unity;

namespace OmniCore.Repository.Sqlite.Repositories
{
    class RadioRepository : Repository<RadioEntity, IRadioEntity>, IRadioRepository
    {
        public RadioRepository(IRepositoryService repositoryService, IUnityContainer container) : base(repositoryService, container)
        {
        }
        public async Task<IRadioEntity> ByDeviceUuid(Guid deviceUuid)
        {
            using var access = await RepositoryService.GetAccess(CancellationToken.None);
            return await access.Connection.Table<RadioEntity>().FirstOrDefaultAsync(x => x.DeviceUuid == deviceUuid);
        }
    }
}
