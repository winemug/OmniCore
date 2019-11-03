using OmniCore.Repository.Entities;
using OmniCore.Repository.Enums;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Repository
{
    public class IndividualRepository : SqliteRepositoryWithUpdate<Individual>
    {

#if DEBUG
        protected override async Task MigrateRepository(SQLiteAsyncConnection connection)
        {
            await base.MigrateRepository(connection);

            var rowCount = await connection.Table<Individual>().CountAsync();

            if (rowCount > 0)
                return;

            var individuals = new []
            {
                new Individual { Name = "TestUser", DateOfBirth = DateTimeOffset.UtcNow.AddYears(-20).AddDays(150), ManagedRemotely = false },
                new Individual { Name = "RemoteTestUser", DateOfBirth = DateTimeOffset.UtcNow.AddYears(-8).AddDays(150), ManagedRemotely = true }
            };

            foreach(var individual in individuals)
            {
                await this.Create(individual);
            }
        }
#endif

    }
}
