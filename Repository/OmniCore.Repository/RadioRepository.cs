using OmniCore.Repository.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Repository
{
    public class RadioRepository : SqliteRepositoryWithUpdate<Radio>
    {
        public RadioRepository(SQLiteAsyncConnection connection) : base(connection)
        {
        }

        public async Task<Radio> GetByProviderSpecificId(string providerSpecificId)
        {
            var c = await GetConnection();
            return await c.Table<Radio>().FirstOrDefaultAsync(r => r.ProviderSpecificId == providerSpecificId);
        }
    }
}
