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
        public PodRepository(SQLiteAsyncConnection connection) : base(connection)
        {
        }

        public async Task<List<Pod>> GetActivePods()
        {
            var c = await GetConnection();
            return await c.Table<Pod>()
                .Where(x => !x.Archived)
                .OrderByDescending(x => x.Created)
                .ToListAsync();
        }

        public override async Task<Pod> Create(Pod entity)
        {
            var c = await GetConnection();
            if (await c.Table<Pod>()
                .Where(p => !p.Archived && p.RadioAddress == entity.RadioAddress).FirstOrDefaultAsync() != null)
                throw new Exception("Cannot have more than one active pod with the same radio address");
            return await base.Create(entity);
        }
    }
}
