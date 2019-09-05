using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OmniCore.Impl.Eros;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using SQLite;

namespace OmniCore.Mobile.Repositories
{
    public class ErosPodRepository : SqliteRepository, IPodRepository<ErosPod>
    {
        public async Task<IList<ErosPod>> GetActivePods()
        {
            var c = await GetConnection();
            return await c.Table<ErosPod>()
                .Where(x => !x.Archived)
                .OrderByDescending(x => x.Created)
                .ToListAsync();
        }

        public async Task SavePod(ErosPod pod)
        {
            var c = await GetConnection();
            if (pod.Id == Guid.Empty)
            {
                pod.Id = Guid.NewGuid();
                pod.Created = DateTimeOffset.UtcNow;
                pod.Updated = pod.Created;
            }
            else
            {
                pod.Updated = DateTimeOffset.UtcNow;
            }
            await c.InsertOrReplaceAsync(pod);
        }

        protected override async Task MigrateRepository(SQLiteAsyncConnection connection)
        {
            await connection.CreateTableAsync<ErosPod>();

            await base.MigrateRepository(connection);
        }
    }
}
