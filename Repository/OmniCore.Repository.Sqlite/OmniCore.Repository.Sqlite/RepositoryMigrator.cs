using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Common.Data.Repositories;
using OmniCore.Repository.Sqlite.Entities;
using SQLite;

namespace OmniCore.Repository.Sqlite
{
    public class RepositoryMigrator : IRepositoryMigrator
    {
        public async Task ExecuteMigration(Version migrateTo, string path,
            CancellationToken cancellationToken)
        {
            var repoVersion = await GetRepositoryVersion(path, cancellationToken);

            if (repoVersion == migrateTo)
            {
                return;
            }

            var tempTargetPath = path + ".tmp";
            if (File.Exists(tempTargetPath))
                File.Delete(tempTargetPath);

            await InitializeDatabase(tempTargetPath, migrateTo, cancellationToken);
            
            if (repoVersion != null)
            {
                await ImportRepository(path, repoVersion, migrateTo, tempTargetPath, cancellationToken);
            }
            
            cancellationToken.ThrowIfCancellationRequested();

            if (File.Exists(path))
            {
                var newPath = path + ".before_upgrade";
                if (File.Exists(newPath))
                    File.Delete(newPath);
                
                File.Move(path, newPath);
            }

            File.Move(tempTargetPath, path);
        }

        public async Task ImportRepository(string importPath, string targetPath, Version migrateTo, CancellationToken cancellationToken)
        {
            var repoVersion = await GetRepositoryVersion(importPath, cancellationToken);
            if (repoVersion == null)
                return;

            await ImportRepository(importPath, repoVersion, migrateTo, targetPath, cancellationToken);
        }
        private async Task<Version> GetRepositoryVersion(string path, CancellationToken cancellationToken)
        {
            if (!File.Exists(path))
                return null;

            var connection = new SQLiteAsyncConnection
                (path, SQLiteOpenFlags.ReadOnly);

            var rc = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM sqlite_master WHERE type=? AND name=?",
                "table", "ErosMessageExchangeResult");

            if (rc > 0)
                return new Version(1,0,0,702);

            rc = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM sqlite_master WHERE type=? AND name=?",
                "table", "MigrationHistory");

            if (rc == 0)
                return null;

            var lastHistoryRecord = await connection.Table<MigrationHistoryEntity>().OrderByDescending(mh => mh.Created)
                .Take(1).FirstOrDefaultAsync();
            if (lastHistoryRecord == null)
                return null;

            return new Version(lastHistoryRecord.ToMajor, lastHistoryRecord.ToMinor, lastHistoryRecord.ToBuild,
                lastHistoryRecord.ToRevision);
        }
        
        private async Task InitializeDatabase(string path, Version version, CancellationToken cancellationToken)
        {
            var connection = new SQLiteAsyncConnection
                (path, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.ProtectionComplete);

            await connection.CreateTableAsync<MigrationHistoryEntity>();
            await connection.CreateTableAsync<PodEntity>();
            await connection.CreateTableAsync<RadioEntity>();
            await connection.CreateTableAsync<RadioEventEntity>();
            await connection.CreateTableAsync<SignalStrengthEntity>();
        }

        private async Task ImportRepository(string importPath, Version fromVersion, Version toVersion, string targetPath, CancellationToken cancellationToken)
        {
            if (fromVersion.Major == 1 &&
                fromVersion.Minor == 0 &&
                fromVersion.Build == 0)
            {
                await Import_1_0_0_x(importPath, fromVersion, toVersion, targetPath, cancellationToken);
            }
            else if (fromVersion.Major == 1 &&
                     fromVersion.Minor == 0 &&
                     fromVersion.Build == 1 &&
                     fromVersion.Revision >= 1201 &&
                     fromVersion.Revision <= 1202)
            {
                await Import_1_0_1_1201(importPath, fromVersion, toVersion, targetPath, cancellationToken);
            }
            else
            {
                throw new OmniCoreRepositoryException(FailureType.RepositoryGeneralError,
                    $"Repository upgrade failed. don't know how to update from application version {fromVersion}");
            }
        }

        private Task Import_1_0_0_x(string importPath, Version fromVersion, Version toVersion, string targetPath,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        
        private Task Import_1_0_1_1201(string importPath, Version fromVersion, Version toVersion, string targetPath,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

    }
}