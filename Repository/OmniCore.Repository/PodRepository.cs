using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OmniCore.Repository.Entities;
using SQLite;

namespace OmniCore.Repository
{
    public class PodRepository : SqliteRepositoryWithUpdate<Pod>
    {
        public async Task<List<Pod>> GetActivePods()
        {
            var c = await GetConnection();
            return await c.Table<Pod>()
                .Where(x => !x.Archived)
                .OrderByDescending(x => x.Created)
                .ToListAsync();
        }
    }
}
