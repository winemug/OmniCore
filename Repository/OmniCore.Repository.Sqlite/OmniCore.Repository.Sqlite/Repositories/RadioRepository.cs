using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform.Common.Data;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using OmniCore.Model.Interfaces.Platform.Common.Data.Repositories;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Repository.Sqlite.Entities;
using SQLite;

namespace OmniCore.Repository.Sqlite.Repositories
{
    class RadioRepository : Repository<RadioEntity, IRadioEntity>, IRadioRepository
    {
        public RadioRepository(IRepositoryService repositoryService) : base(repositoryService)
        {
        }
        public async Task<IRadioEntity> ByDeviceUuid(Guid deviceUuid, CancellationToken cancellationToken)
        {
            return await DataTask((c) =>
            {
                return c.Table<RadioEntity>()
                    .FirstOrDefaultAsync(x => x.DeviceUuid == deviceUuid);
            }, cancellationToken);
        }

#if DEBUG
        public override async Task EnsureSchemaAndDefaults(CancellationToken cancellationToken)
        {
            await base.EnsureSchemaAndDefaults(cancellationToken);

            var radioId = Guid.Parse("00000000-0000-0000-0000-000780393d00");

            var r = await ByDeviceUuid(radioId, cancellationToken);
            if (r == null)
            {
                await DataTask((c) =>
                {
                    var radio = New();
                    radio.DeviceUuid = radioId;
                    radio.ServiceUuids = new[]
                    {
                        Guid.Parse("0235733b-99c5-4197-b856-69219c2a3845")
                    };
                    radio.Created = DateTimeOffset.UtcNow;
                    return c.InsertAsync(radio);
                }, cancellationToken);
            }
        }
#endif
    }
}
