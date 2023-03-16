using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using OmniCore.Services.Entities;

namespace OmniCore.Services;

//public class BgcService
//{
//    [Dependency] public ConfigurationService ConfigurationService { get; set; }

//    [Dependency] public DataService DataService { get; set; }

//    [Dependency] public SyncClient SyncClient { get; set; }

//    public async Task InitializeAsync()
//    {
//        await QueueAllUnsentReadingsAsync();
//    }

//    public async Task AddReadingsAsync(IEnumerable<BgcEntry> bgcReadings)
//    {
//        using (var conn = await DataService.GetConnectionAsync())
//        {
//            foreach (var reading in bgcReadings)
//                try
//                {
//                    var e = await CheckAddBgcReadingAsync(conn, reading);
//                    if (e != null)
//                    {
//                        Debug.WriteLine("Reading doesn't exist, queuing for sync.");
//                        await SyncClient.EnqueueAsync(e);
//                    }
//                }
//                catch (Exception e)
//                {
//                    Debug.WriteLine($"Error Adding Readings {e}");
//                    throw;
//                }
//        }
//    }

//    public async Task SetSyncedAsync(BgcEntry entry, long dbRowId, bool isSynced)
//    {
//        using (var conn = await DataService.GetConnectionAsync())
//        {
//            await conn.ExecuteAsync("UPDATE bgc SET synced=@synced WHERE rowid=@rowid", new
//            {
//                rowid = dbRowId,
//                synced = isSynced ? 1 : 0
//            });
//        }
//    }

//    public async Task<DateTimeOffset?> GetLastReadingDateAsync(Guid profileId, Guid clientId)
//    {
//        using (var conn = await DataService.GetConnectionAsync())
//        {
//            var row = await conn.QueryFirstOrDefaultAsync(
//                "SELECT date FROM bgc WHERE deleted = 0 AND client_id = @cid AND profile_id = @pid ORDER BY date DESC",
//                new
//                {
//                    cid = clientId.ToString("N"),
//                    pid = profileId.ToString("N")
//                });
//            if (row == null)
//                return null;
//            long dt = row.date;
//            return DateTimeOffset.FromUnixTimeMilliseconds(dt);
//        }
//    }

//    private async Task<BgcEntry> CheckAddBgcReadingAsync(SqliteConnection conn, BgcEntry bgce)
//    {
//        var count = await conn.ExecuteScalarAsync<int>(
//            @"SELECT COUNT(*) FROM bgc WHERE profile_id = @pid AND client_id = @cid AND date = @date",
//            new
//            {
//                pid = bgce.ProfileId.ToString("N"),
//                cid = bgce.ClientId.ToString("N"),
//                date = bgce.Date.ToUnixTimeMilliseconds()
//            });
//        if (count > 0)
//            return null;
//        using (var tran = conn.BeginTransaction())
//        {
//            await conn.ExecuteAsync("INSERT INTO bgc(profile_id, client_id, date, " +
//                                    "type, direction, value, rssi, synced, deleted) VALUES(@pid, @cid, @date, @type, @dir, @val, @rssi, 0, 0)",
//                new
//                {
//                    pid = bgce.ProfileId.ToString("N"),
//                    cid = bgce.ClientId.ToString("N"),
//                    date = bgce.Date.ToUnixTimeMilliseconds(),
//                    type = (int?)bgce.Type,
//                    dir = (int?)bgce.Direction,
//                    val = bgce.Value,
//                    rssi = bgce.Rssi
//                }
//            );
//            long dbRowId = await conn.ExecuteScalarAsync<int>("SELECT last_insert_rowid()");
//            bgce.SetSyncedTask = new Task(async () => { await SetSyncedAsync(bgce, dbRowId, true); });
//            tran.Commit();
//        }

//        return bgce;
//    }

//    private async Task QueueAllUnsentReadingsAsync()
//    {
//        var cc = await ConfigurationService.GetConfigurationAsync();
//        using (var conn = await DataService.GetConnectionAsync())
//        {
//            var reader = await conn.ExecuteReaderAsync(
//                "SELECT rowid, profile_id, client_id, date, type, direction, value, rssi, deleted FROM bgc " +
//                " WHERE synced = 0 AND client_id = @cid",
//                new
//                {
//                    cid = cc.ClientId.Value.ToString("N")
//                });
//            while (await reader.ReadAsync())
//            {
//                var bgce = new BgcEntry
//                {
//                    ProfileId = Guid.Parse(reader.GetString(1)),
//                    ClientId = Guid.Parse(reader.GetString(2)),
//                    Date = DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64(3)),
//                    Type = (BgcReadingType?)reader.GetFieldValue<int?>(4),
//                    Direction = (BgcDirection?)reader.GetFieldValue<int?>(5),
//                    Value = reader.GetDouble(6),
//                    Rssi = reader.GetFieldValue<int?>(7),
//                    Deleted = reader.GetInt32(8) == 1
//                };
//                var dbRowId = reader.GetInt64(0);
//                bgce.SetSyncedTask = new Task(async () => { await SetSyncedAsync(bgce, dbRowId, true); });
//                await SyncClient.EnqueueAsync(bgce);
//            }
//        }
//    }
//}