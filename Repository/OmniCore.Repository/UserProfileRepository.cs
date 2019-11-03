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
            var mq = await mr.ForQuery();
            var urq = await ur.ForQuery();

            var localUser = await urq.FirstAsync(u => u.Name == "TestUser");

            var med = await mq.FirstOrDefaultAsync(m => m.Hormone == HormoneType.Insulin);

            var profile = await upr.Create(new UserProfile { UserId = localUser.Id.Value, MedicationId = med.Id.Value, PodBasalSchedule = new [] 
                {1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m,
                  1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m} });
            await upr.Create(profile);

        }
#endif
    }
}
