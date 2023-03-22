using System;
using System.Diagnostics;
using Dapper;
using Microsoft.Data.Sqlite;
using Nito.AsyncEx;
using OmniCore.Services.Data.Sql;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class DataService : IDataService
{
    private AsyncManualResetEvent _databaseInitializedEvent = new AsyncManualResetEvent();
    private bool _initialized;
    public DataService()
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        DatabasePath = Path.Combine(basePath, "omnicore.db3");
        _initialized = false;
    }

    public string DatabasePath { get; }

    public async Task Start()
    {
        await InitializeDatabaseAsync();
    }

    public async Task Stop()
    {
    }

    public async Task<SqliteConnection> GetConnectionAsync()
    {
        if (!_initialized)
            await _databaseInitializedEvent.WaitAsync();
        var conn = new SqliteConnection($"Data Source={DatabasePath};Cache=Shared");
        await conn.OpenAsync();
        return conn;
    }

    public async Task InitializeDatabaseAsync()
    {
        using (var conn = new SqliteConnection($"Data Source={DatabasePath};Cache=Shared"))
        {
            var storedVersion = -1;
            try
            {
                await conn.OpenAsync();
                await conn.ExecuteAsync("PRAGMA journal_mode=WAL;");
                var row = await conn.QueryFirstOrDefaultAsync(
                    "SELECT db_version FROM version");
                storedVersion = (int)row.db_version;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Error retrieving db version -- reinitializing. {e}");
            }

            if (storedVersion != DatabaseMigration.LatestVersion)
            {
                Trace.WriteLine("DB migration started");
                await DatabaseMigration.RunMigration(conn, storedVersion);
                Trace.WriteLine("DB migration ended");
            }
        }
        _databaseInitializedEvent.Set();
        _initialized = true;
        Trace.WriteLine("DB initialized");
    }

    public async Task CopyDatabase(string destinationPath)
    {
        // try
        // {
        //     using (var _ = await _initializeLock.LockAsync())
        //     {
        //         using (var source = File.Open(DatabasePath, FileMode.Open))
        //         {
        //             using (var dest = File.Create(destinationPath))
        //             {
        //                 await source.CopyToAsync(dest);
        //             }
        //         }
        //     }
        // }
        // catch (Exception e)
        // {
        //     Trace.WriteLine($"Database Copy failed {e}");
        //     throw;
        // }
        // finally
        // {
        //     _initialized = wasInitialized;
        // }
    }

    public async Task CreatePodMessage(Guid podId, Guid clientId, int recordIndex, DateTimeOffset sendStart, DateTimeOffset receiveEnd, byte[] sentData,
        byte[] receivedData, ExchangeResult result)
    {
        using (var conn = await GetConnectionAsync())
        {
            await conn.ExecuteAsync(
                "INSERT INTO pod_message(pod_id, client_id, record_index, send_start, send_data, " +
                "receive_end, receive_data, exchange_result) " +
                "VALUES(@pod_id, @client_id, @record_index, @send_start, @send_data, " +
                "@receive_end, @receive_data, @exchange_result)",
                new
                {
                    pod_id = podId.ToString("N"),
                    client_id = clientId.ToString("N"),
                    record_index = recordIndex,
                    send_start = sendStart.ToUnixTimeMilliseconds(),
                    send_data = sentData,
                    receive_end = receiveEnd.ToUnixTimeMilliseconds(),
                    receive_data = receivedData,
                    exchange_result = (int)result.Error
                });
        }
    }
}