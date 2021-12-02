using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;

namespace OmniCore.Services.Tables
{
    public static class BgcReceiverTable
    {
        public static async Task Create(SqliteConnection conn)
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

        public static async Task ResetUpdates(SqliteConnection conn)
        {
            throw new System.NotImplementedException();
        }

        public static async Task CleanupDeleted(SqliteConnection conn)
        {
            throw new System.NotImplementedException();
        }
    }
}