using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Nito.AsyncEx;
using OmniCore.Services.Data.Sql;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Core;

namespace OmniCore.Services;

public class DataService : IDataService
{
    private bool _initialized;
    private readonly AsyncLock _initializeLock = new();
    private Task _startupTask;

    public DataService()
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        DatabasePath = Path.Combine(basePath, "omnicore.db3");
    }

    public string DatabasePath { get; }

    public async Task Start()
    {
        _startupTask = Task.Run(async () => await InitializeDatabaseAsync());
    }

    public async Task Stop()
    {
        try
        {
            _startupTask.GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            Trace.WriteLine($"Error stopping Dataservice, startup task reports: {e}");
            throw;
        }
    }

    public async Task<SqliteConnection> GetConnectionAsync()
    {
        if (!_initialized)
            await InitializeDatabaseAsync();
        var conn = new SqliteConnection($"Data Source={DatabasePath}");
        await conn.OpenAsync();
        return conn;
    }

    public async Task InitializeDatabaseAsync()
    {
        using (var _ = await _initializeLock.LockAsync())
        {
            if (!_initialized)
            {
                using (var conn = new SqliteConnection($"Data Source={DatabasePath}"))
                {
                    var storedVersion = -1;
                    try
                    {
                        await conn.OpenAsync();
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

                _initialized = true;
                Trace.WriteLine("DB initialized");
            }
        }
    }

    public async Task CopyDatabase(string destinationPath)
    {
        if (_initialized)
        {
            _initialized = false;
            try
            {
                using (var _ = await _initializeLock.LockAsync())
                {
                    using (var source = File.Open(DatabasePath, FileMode.Open))
                    {
                        using (var dest = File.Create(destinationPath))
                        {
                            await source.CopyToAsync(dest);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Database Copy failed {e}");
                throw;
            }
            finally
            {
                _initialized = true;
            }
        }
    }
}