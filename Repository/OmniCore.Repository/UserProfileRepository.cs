using OmniCore.Repository.Enums;
using OmniCore.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace OmniCore.Repository
{
    public class UserProfileRepository : SqliteRepositoryWithUpdate<UserProfile>
    {
        public UserProfileRepository(SQLiteAsyncConnection connection) : base(connection)
        {
        }

        public async Task<List<UserProfile>> GetProfilesByMedication(long userId, long medicationId)
        {
            var c = await GetConnection();
            return await c.Table<UserProfile>().Where(p => p.MedicationId == medicationId && p.UserId == userId).ToListAsync();
        }
    }
}
