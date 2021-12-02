using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;

namespace OmniCore.Services.Data.Tables
{
    public class ProfileTable : IDataTable
    {
        public async Task Create(SqliteConnection conn)
        {
            await conn.ExecuteAsync(
                @"CREATE TABLE profile (
                            id TEXT PRIMARY KEY,
                            name TEXT,
                            updated INTEGER NOT NULL,
                            deleted INTEGER NOT NULL
                            ");
        }

        public async Task ResetUpdates(SqliteConnection conn)
        {
            await conn.ExecuteAsync(@"UPDATE profile SET updated = 1 WHERE updated <> 0");
        }

        public async Task CleanupDeleted(SqliteConnection conn)
        {
            await conn.ExecuteAsync(@"DELETE FROM profile WHERE deleted = 1 AND updated = 0");
        }
    }
}