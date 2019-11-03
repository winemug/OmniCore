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
#if DEBUG
        protected override async Task MigrateRepository(SQLiteAsyncConnection connection)
        {
            await base.MigrateRepository(connection);

            var rowCount = await connection.Table<UserProfile>().CountAsync();

            if (rowCount > 0)
                return;

            using var ur = new UserRepository();
            using var mr = new MedicationRepository();
            using var upr = new UserProfileRepository();

            var localUser = await ur.GetUserByName("TestUser");

            var meds = await mr.GetMedicationByHormone(HormoneType.Insulin);

            var profile = await upr.Create(new UserProfile { UserId = localUser.Id.Value, MedicationId = meds[0].Id.Value, PodBasalSchedule = new [] 
                {1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m,
                  1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m} });
            await upr.Create(profile);

        }
#endif

        public async Task<List<UserProfile>> GetProfilesByMedication(long userId, long medicationId)
        {
            var c = await GetConnection();
            return await c.Table<UserProfile>().Where(p => p.MedicationId == medicationId && p.UserId == userId).ToListAsync();
        }
    }
}
