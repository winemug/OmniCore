using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;

namespace OmniCore.Services.Data.Tables
{
    public class BgcReceiverTable : IDataTable
    {
        public async Task Create(SqliteConnection conn)
        {
            await conn.ExecuteAsync(
                @"CREATE TABLE bgc_receiver (
                            id TEXT PRIMARY KEY,
                            client_id TEXT,
                            profile_id TEXT,
                            type TEXT,
                            updated INTEGER NOT NULL,
                            deleted INTEGER NOT NULL
                            ");
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