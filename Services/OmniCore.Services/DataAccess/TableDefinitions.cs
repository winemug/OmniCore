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
    synced INTEGER DEFAULT 0 NOT NULL,
    deleted INTEGER DEFAULT 0 NOT NULL
);

DROP TABLE IF EXISTS pod;
CREATE TABLE pod
(
    id TEXT NOT NULL,
    radio_address INTEGER NOT NULL,
    units_per_ml INTEGER NOT NULL,
    medication INTEGER NOT NULL,
    lot INTEGER,
    serial INTEGER,
    progress INTEGER NOT NULL,
    packet_sequence INTEGER NOT NULL,
    message_sequence INTEGER NOT NULL,
    entered INTEGER NOT NULL,
    removed INTEGER,
    synced INTEGER DEFAULT 0 NOT NULL
);

DROP TABLE IF EXISTS version;
CREATE TABLE version
(
    db_version TEXT NOT NULL
);

CREATE UNIQUE INDEX bgc_reading_date ON bgc(profile_id, client_id, date);
CREATE INDEX bgc_date ON bgc(profile_id, date);

";
            await conn.ExecuteAsync(sql);
        }
    }
}