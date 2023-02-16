using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    public class DataStore
    {
        public string DatabasePath { get; }
        private bool _initialized = false;
        private const string DbVersion = "000";
        private AsyncLock _initializeLock = new AsyncLock();
        public DataStore()
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            DatabasePath = Path.Combine(basePath, "omnicore.db3");
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
                            await MigrateDatabaseAsync(storedVersion);
                        }
                    }
                    else
                    {
                        await CreateDatabaseAsync();
                    }
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
            if (File.Exists(DatabasePath))
                File.Delete(DatabasePath);
            
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
