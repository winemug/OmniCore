using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;

namespace OmniCore.Services.Tables
{
    public static class TableDefinitions
    {
        public static async Task RunCreate(SqliteConnection conn)
        {
            
            var sql = @"

PRAGMA journal_mode = 'wal';

DROP TABLE IF EXISTS client;
CREATE TABLE client
(
    id TEXT NOT NULL,
    name TEXT NOT NULL,
    sw_version TEXT,
    hw_version TEXT,
    os_version TEXT,
    platform TEXT
);
                    
DROP TABLE IF EXISTS profile;
CREATE TABLE profile
(
    id TEXT PRIMARY KEY,
    name TEXT
);

DROP TABLE IF EXISTS bgc;
CREATE TABLE bgc
(
    profile_id TEXT NOT NULL,
    client_id TEXT NOT NULL,
    date INTEGER NOT NULL,
    type INTEGER,
    direction INTEGER,
    value REAL,
    rssi INTEGER,
    synced INTEGER,
    deleted INTEGER
);

CREATE UNIQUE INDEX bgc_reading_date ON bgc(profile_id, client_id, date);

";
            await conn.ExecuteAsync(sql);
        }
    }
}