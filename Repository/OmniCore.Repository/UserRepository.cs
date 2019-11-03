using OmniCore.Repository.Entities;
using OmniCore.Repository.Enums;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Repository
{
    public class UserRepository : SqliteRepositoryWithUpdate<User>
    {

#if DEBUG
        protected override async Task MigrateRepository(SQLiteAsyncConnection connection)
        {
            await base.MigrateRepository(connection);

            var rowCount = await connection.Table<User>().CountAsync();

            if (rowCount > 0)
                return;

            var localUser = await this.Create(new User { Name = "TestUser", DateOfBirth = DateTimeOffset.UtcNow.AddYears(-20).AddDays(150), ManagedRemotely = false });
            var remoteUser = await this.Create(new User { Name = "RemoteTestUser", DateOfBirth = DateTimeOffset.UtcNow.AddYears(-8).AddDays(150), ManagedRemotely = true });

            using var mr = new MedicationRepository();
            using var upr = new UserProfileRepository();

            var mq = await mr.ForQuery();
            var med = await mq.FirstOrDefaultAsync(m => m.Hormone == HormoneType.Insulin);

            var profile = await upr.Create(new UserProfile { UserId = localUser.Id.Value, MedicationId = med.Id.Value, PodBasalSchedule = new [] 
                {1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m,
                  1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m} });
            await upr.Create(profile);
        }
#endif

    }
}
