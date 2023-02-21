using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Nito.AsyncEx;
using OmniCore.Services.Entities;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Tables;
using Unity;
using Xamarin.Forms;

namespace OmniCore.Services
{
    public class DataService
    {
        public string DatabasePath { get; }
        private bool _initialized = false;
        private const string DbVersion = "9";
        private AsyncLock _initializeLock = new AsyncLock();
        private Task _startupTask;
        public DataService()
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            DatabasePath = Path.Combine(basePath, "omnicore.db3");
        }

        public void Start()
        {
            _startupTask = Task.Run(async () => await InitializeDatabaseAsync());
        }

        public void Stop()
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
                    if (File.Exists(DatabasePath))
                    {
                        string storedVersion = null;
                        try
                        {
                            using (var conn = new SqliteConnection($"Data Source={DatabasePath}"))
                            {
                                await conn.OpenAsync();
                                var row = await conn.QueryFirstOrDefaultAsync(
                                    "SELECT db_version FROM version");
                                storedVersion = row.db_version;
                            }
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine($"Error retrieving db version -- reinitializing. {e}");
                        }

                        if (string.IsNullOrEmpty(storedVersion) || storedVersion != DbVersion)
                        {
                            Trace.WriteLine($"DB migration started");
                            await MigrateDatabaseAsync(storedVersion);
                            Trace.WriteLine($"DB migration ended");
                        }
                    }
                    else
                    {
                        Trace.WriteLine($"DB creation started");
                        await CreateDatabaseAsync();
                        Trace.WriteLine($"DB creation ended");
                    }
                    _initialized = true;
                    Trace.WriteLine($"DB initialized");
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

        private async Task MigrateDatabaseAsync(string existingVersion)
        {
            // initial non-migration
            await CreateDatabaseAsync();
        }
        
        private async Task CreateDatabaseAsync()
        {
            // if (File.Exists(DatabasePath))
            //     File.Delete(DatabasePath);
            
            using (var conn = new SqliteConnection($"Data Source={DatabasePath}"))
            {
                await conn.OpenAsync();
                await TableDefinitions.RunCreate(conn);
                await conn.ExecuteAsync("INSERT INTO version(db_version) VALUES(@version)",
                    new { version = DbVersion });
            }
        }
    }
}
