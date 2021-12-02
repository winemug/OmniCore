using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;

namespace OmniCore.Services.Tables
{
    public static class ApplicationTable
    {
        public static async Task Create(SqliteConnection conn)
        {
            await conn.ExecuteAsync(
                @"CREATE TABLE application (
                        client_id TEXT NOT NULL,
                        client_name TEXT NOT NULL,
                        client_key BLOB,
                        account_id BLOB,
                        db_version TEXT,
                        updated INTEGER NOT NULL,
                        deleted INTEGER NOT NULL
                    )");
            await conn.ExecuteAsync(
                "INSERT INTO application(client_id, db_version, updated, deleted) VALUES(@clientId, @dbVersion, 1, 0)",
                new
                {
                    clientId = Guid.NewGuid().ToByteArray(),
                    dbVersion = 1,
                });
        }

        public static async Task ResetUpdates(SqliteConnection conn)
        {
            await conn.ExecuteAsync(@"UPDATE application SET updated = 1 WHERE updated <> 0");
        }

        public static async Task CleanupDeleted(SqliteConnection conn)
        {
            // no deletion
        }
    }
}