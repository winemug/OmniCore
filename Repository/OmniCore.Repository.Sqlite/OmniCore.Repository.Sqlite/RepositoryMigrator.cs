using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            if (!File.Exists(path))
            {
                await InitializeDatabase(path, migrateTo, cancellationToken);
            }
            else
            {
                var repoVersion = await GetRepositoryVersion(path, cancellationToken);

                if (repoVersion == null)
                {
                    //TODO: move away but notify
                    var backupPath = path + ".couldntupgrade";
                    if (File.Exists(backupPath))
                        File.Delete(backupPath);
                    File.Move(path, backupPath);

                    await InitializeDatabase(path, migrateTo, cancellationToken);
                }
                else
                {
                    while (repoVersion != migrateTo)
                    {
                        repoVersion = await MigrateDatabase(path, repoVersion, migrateTo, cancellationToken);
                    }
                }
            }
        }

        public async Task<Version> MigrateDatabase(string path, Version fromVersion, Version toVersion, CancellationToken cancellationToken)
        {

            if (fromVersion.Major == 1 &&
                fromVersion.Minor == 0 &&
                fromVersion.Build == 0)
            {
                //TODO:
                return new Version(1,0,1,1300);
            }

            if (fromVersion.Major == 1 &&
                fromVersion.Minor == 0 &&
                fromVersion.Build == 1 &&
                fromVersion.Build < 1400)
            {
                await InitializeDatabase(path, toVersion, cancellationToken);
                return toVersion;
            }

            throw new OmniCoreRepositoryException(FailureType.RepositoryGeneralError,
                $"Repository upgrade failed. don't know how to update from application version {fromVersion}");
        }

        public async Task ImportRepository(string importPath, string targetPath, Version migrateTo, CancellationToken cancellationToken)
        {
            var repoVersion = await GetRepositoryVersion(importPath, cancellationToken);
            if (repoVersion == null)
                throw new OmniCoreRepositoryException(FailureType.RepositoryGeneralError,
                    $"Repository import failed. Import source is not a valid omnicore database.");

            throw new NotImplementedException();
        }

        private async Task<Version> GetRepositoryVersion(string path, CancellationToken cancellationToken)
        {
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
            var mhe = await connection.Table<MigrationHistoryEntity>()
                .OrderByDescending(mh => mh.Created)
                .FirstOrDefaultAsync();

            if (mhe == null || mhe.ToMajor != version.Major || mhe.ToMinor != version.Minor
                || mhe.ToBuild != version.Build || mhe.ToRevision != version.Revision)
            {
                await connection.InsertAsync(new MigrationHistoryEntity
                {
                    Created = DateTimeOffset.UtcNow,
                    ImportPath = path,
                    IsDeleted = false,
                    ToMajor = version.Major,
                    ToMinor = version.Minor,
                    ToRevision = version.Revision,
                    ToBuild = version.Build
                });
            }

            await connection.CreateTableAsync<PodEntity>();
            await connection.CreateTableAsync<RadioEntity>();
            await connection.CreateTableAsync<RadioEventEntity>();
            await connection.CreateTableAsync<SignalStrengthEntity>();
        }

        //private List<(Func<Version, bool> Predicate, Func<string, Version> Migration)> GetMigrationEvaluators()
        //{
        //}

        //private async Task ImportRepository(string importPath, Version fromVersion, Version toVersion, string targetPath, CancellationToken cancellationToken)
        //{
        //    if (fromVersion.Major == 1 &&
        //        fromVersion.Minor == 0 &&
        //        fromVersion.Build == 0)
        //    {
        //        await Import_1_0_0_x(importPath, fromVersion, toVersion, targetPath, cancellationToken);
        //    }
        //    else if (fromVersion.Major == 1 &&
        //             fromVersion.Minor == 0 &&
        //             fromVersion.Build == 1 &&
        //             fromVersion.Revision <= 1305)
        //    {
        //        await Import_1_0_1_12(importPath, fromVersion, toVersion, targetPath, cancellationToken);
        //    }
        //    else
        //    {
        //        throw new OmniCoreRepositoryException(FailureType.RepositoryGeneralError,
        //            $"Repository upgrade failed. don't know how to update from application version {fromVersion}");
        //    }
        //}

        //private Task Import_1_0_0_x(string importPath, Version fromVersion, Version toVersion, string targetPath,
        //    CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}
        
        //private Task Import_1_0_1_1201(string importPath, Version fromVersion, Version toVersion, string targetPath,
        //    CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}
    }
}