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
        }
#endif

        public async Task<User> GetUserByName(string username)
        {
            var c = await GetConnection();
            return await c.Table<User>().FirstOrDefaultAsync(x => x.Name == username);
        }

        public async Task<List<User>> GetUsers(string username)
        {
            var c = await GetConnection();
            return await c.Table<User>().OrderBy(x => x.Name).ToListAsync();
        }
    }
}
