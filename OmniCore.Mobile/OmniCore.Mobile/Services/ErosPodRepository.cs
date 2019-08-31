using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OmniCore.Impl.Eros;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using SQLite;

namespace OmniCore.Mobile.Services
{
    public class ErosPodRepository : SqliteRepository, IPodRepository<ErosPod>
    {
        public async Task<IList<ErosPod>> GetActivePods()
        {
            var c = await GetConnection();
            await c.CreateTableAsync<ErosPod>();
            return await c.Table<ErosPod>()
                .Where(x => !x.Archived)
                .OrderByDescending(x => x.Created)
                .ToListAsync();
        }

        public async Task SavePod(ErosPod pod)
        {
            var c = await GetConnection();
            await c.CreateTableAsync<ErosPod>();
        }
    }
}
