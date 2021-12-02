using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;

namespace OmniCore.Services.Data.Tables
{
    public class ApplicationTable : IDataTable
    {
        public async Task Create(SqliteConnection conn)
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

        public async Task ResetUpdates(SqliteConnection conn)
        {
            await conn.ExecuteAsync(@"UPDATE application SET updated = 1 WHERE updated <> 0");
        }

        public async Task CleanupDeleted(SqliteConnection conn)
        {
            // no deletion
        }
    }
}