using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using OmniCore.Services.Entities;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Tables;
using Unity;
using Xamarin.Forms;

namespace OmniCore.Services
{
    public class DataStore
    {
        [Unity.Dependency]
        public SyncClient SyncClient { get; set; }
        
        [Unity.Dependency]
        public ConfigurationStore ConfigurationStore { get; set; }

        public string DatabasePath { get; }
        public string DatabaseVersionFilePath { get; }
        private bool _initialized = false;
        private const string DbVersion = "1";
        public DataStore()
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            DatabasePath = Path.Combine(basePath, "omnicore.db3");
            DatabaseVersionFilePath = Path.Combine(basePath, "dbver");
        }
        
        public async Task<SqliteConnection> GetConnectionAsync()
        {
            if (!_initialized)
                await InitializeDatabaseAsync();
            var conn = new SqliteConnection($"Data Source={DatabasePath};Cache=Shared");
            await conn.OpenAsync();
            return conn;
        }

        public async Task InitializeDatabaseAsync()
        {
            if (!_initialized)
            {
                if (File.Exists(DatabasePath))
                {
                    var versionMatch = false;
                    if (File.Exists(DatabaseVersionFilePath))
                    {
                        var dbver = File.ReadAllText(DatabaseVersionFilePath);
                        versionMatch = dbver == DbVersion;
                    }

                    if (!versionMatch)
                    {
                        await MigrateDatabaseAsync();
                    }
                }
                else
                {
                    await CreateDatabaseAsync();
                }
                _initialized = true;
            }
        }
        private async Task MigrateDatabaseAsync()
        {
            // initial non-migration
            await CreateDatabaseAsync();
        }
        
        private async Task CreateDatabaseAsync()
        {
            if (File.Exists(DatabaseVersionFilePath))
                File.Delete(DatabaseVersionFilePath);
            if (File.Exists(DatabasePath))
                File.Delete(DatabasePath);
            
            using (var conn = new SqliteConnection($"Data Source={DatabasePath}"))
            {
                await TableDefinitions.RunCreate(conn);
            }
            
            File.WriteAllText(DatabaseVersionFilePath, DbVersion);
        }

        public async Task AddReadingsAsync(IEnumerable<BgcEntry> bgcReadings)
        {
            using (var conn = await GetConnectionAsync())
            {
                foreach (var reading in bgcReadings)
                {
                    // Debug.WriteLine($"Checking reading {reading.Date} {reading.Value}");
                    try
                    {
                        var e = await AddBgcReadingAsync(conn, reading); 
                        if (e != null) 
                        {
                            Debug.WriteLine($"Reading doesn't exist, queuing for sync.");
                            await SyncClient.EnqueueAsync(e);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
        }

        public async Task<DateTimeOffset?> GetLastReadingDateAsync(Guid profileId, Guid clientId)
        {
            using (var conn = await GetConnectionAsync())
            {
                var row = await conn.QueryFirstOrDefaultAsync(
                    "SELECT date FROM bgc WHERE deleted = 0 AND client_id = @cid AND profile_id = @pid ORDER BY date DESC",
                    new
                    {
                        cid = clientId.ToString("N"),
                        pid = profileId.ToString("N")
                    });
                if (row == null)
                    return null;
                long dt = row.date;
                return DateTimeOffset.FromUnixTimeMilliseconds(dt);
            }
        }
        public async Task EnqueueReadingsAsync()
        {
            var cc = await ConfigurationStore.GetConfigurationAsync();
            using (var conn = await GetConnectionAsync())
            {
                var reader = await conn.ExecuteReaderAsync(
                    "SELECT rowid, profile_id, client_id, date, type, direction, value, rssi, deleted FROM bgc " +
                    " WHERE synced = 0 AND client_id = @cid",
                    new
                    {
                        cid = cc.ClientId.Value.ToString("N")
                    });
                while(await reader.ReadAsync())
                {
                    var bgce = new BgcEntry()
                    {
                        DbRowId = reader.GetInt64(0),
                        ProfileId = Guid.Parse(reader.GetString(1)),
                        ClientId = Guid.Parse(reader.GetString(2)),
                        Date = DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64(3)),
                        Type = (BgcReadingType?)reader.GetFieldValue<int?>(4),
                        Direction = (BgcDirection?)reader.GetFieldValue<int?>(5),
                        Value = reader.GetDouble(6),
                        Rssi = reader.GetFieldValue<int?>(7),
                        Deleted = reader.GetInt32(8) == 1,
                    };
                    await SyncClient.EnqueueAsync(bgce);
                }
            }
        }
        
        private async Task<BgcEntry> AddBgcReadingAsync(SqliteConnection conn, BgcEntry bgcr)
        {
            var count = await conn.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*) FROM bgc WHERE profile_id = @pid AND client_id = @cid AND date = @date",
                new
                {
                    pid = bgcr.ProfileId.ToString("N"),
                    cid = bgcr.ClientId.ToString("N"),
                    date = bgcr.Date.ToUnixTimeMilliseconds()
                });
            if (count > 0)
                return null;
            await conn.ExecuteAsync("INSERT INTO bgc(profile_id, client_id, date, " +
                                    "type, direction, value, rssi, synced, deleted) VALUES(@pid, @cid, @date, @type, @dir, @val, @rssi, 0, 0)",
                new
                {
                    pid = bgcr.ProfileId.ToString("N"),
                    cid = bgcr.ClientId.ToString("N"),
                    date = bgcr.Date.ToUnixTimeMilliseconds(),
                    type = (int?)bgcr.Type,
                    dir = (int?)bgcr.Direction,
                    val = bgcr.Value,
                    rssi = bgcr.Rssi
                }
            );
            bgcr.DbRowId = await conn.ExecuteScalarAsync<int>("SELECT last_insert_rowid()");
            return bgcr;
        }

        public async Task SetSyncedAsync(ISyncableEntry entry, bool b)
        {
            using (var conn = await GetConnectionAsync())
            {
                await conn.ExecuteAsync($"UPDATE {entry.DbTableName} SET synced=@synced WHERE rowid=@DbRowId", new
                {
                    DbRowId = entry.DbRowId,
                    synced = b ? 1 : 0
                });
            }
        }
    }
}
