using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;

namespace OmniCore.Services.Tables
{
    public class BgcReadingTable
    {
        public static async Task Create(SqliteConnection conn)
        {
            await conn.ExecuteAsync(
                @"CREATE TABLE bgc_reading (
                    receiver_id TEXT NOT NULL,
                    date INTEGER NOT NULL,
                    direction TEXT,
                    value INTEGER NOT NULL,
                    updated INTEGER NOT NULL,
                    deleted INTEGER NOT NULL
                )");
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