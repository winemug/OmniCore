using System;
using System.Collections.Generic;
using System.Text;
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
        public RadioRepository(IDataAccess dataAccess, IUnityContainer container) : base(dataAccess, container)
        {
        }
        public async Task<IRadioEntity> ByDeviceUuid(Guid deviceUuid)
        {
            return await Connection.Table<RadioEntity>().FirstOrDefaultAsync(x => x.DeviceUuid == deviceUuid);
        }
    }
}
