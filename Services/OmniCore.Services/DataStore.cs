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
    }
}
