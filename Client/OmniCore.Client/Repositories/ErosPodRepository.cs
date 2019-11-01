using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OmniCore.Impl.Eros;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using SQLite;

namespace OmniCore.Client.Repositories
{
    public class ErosPodRepository : SqliteRepository<ErosPod>, IPodRepository<ErosPod>
    {
        public async Task<IList<ErosPod>> GetActivePods()
        {
            var c = await GetConnection();
            return await c.Table<ErosPod>()
                .Where(x => !x.Archived)
                .OrderByDescending(x => x.Created)
                .ToListAsync();
        }
    }
}
