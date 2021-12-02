using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;

namespace OmniCore.Services.Data.Tables
{
    public class BgcTable : IDataTable
    {
        public async Task Create(SqliteConnection conn)
        {
            await conn.ExecuteAsync(
                @"CREATE TABLE bgc (
                    receiver_id TEXT NOT NULL,
                    date INTEGER NOT NULL,
                    direction TEXT,
                    value INTEGER NOT NULL,
                    updated INTEGER NOT NULL,
                    deleted INTEGER NOT NULL
                )");
        }

        public async Task ResetUpdates(SqliteConnection conn)
        {
            throw new System.NotImplementedException();
        }

        public async Task CleanupDeleted(SqliteConnection conn)
        {
            throw new System.NotImplementedException();
        }
    }
}