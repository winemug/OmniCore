using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Data.Repositories;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using OmniCore.Model.Interfaces.Platform.Common.Data.Repositories;
using OmniCore.Model.Interfaces.Platform.Server;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Repository.Sqlite.Entities;
using OmniCore.Repository.Sqlite.Repositories;
using SQLite;

namespace OmniCore.Repository.Sqlite
{
    public class RepositoryMigrator : IRepositoryMigrator
    {
        private readonly ICoreNotificationFunctions NotificationFunctions;
        private readonly ICoreContainer<IServerResolvable> ServerContainer;
        
        public RepositoryMigrator(ICoreNotificationFunctions notificationFunctions,
            ICoreContainer<IServerResolvable> serverContainer)
        {
            NotificationFunctions = notificationFunctions;
            ServerContainer = serverContainer;
        }
        
        public async Task ExecuteMigration(Version targetVersion, IRepositoryAccessProvider targetAccessProvider,
            CancellationToken cancellationToken)
        {
            ICoreNotification migrationInformation;
            
#if DEBUG
            if (File.Exists(targetAccessProvider.DataPath))
                File.Delete(targetAccessProvider.DataPath);
#endif
            
            if (!File.Exists(targetAccessProvider.DataPath))
            {
                migrationInformation = NotificationFunctions.CreateNotification(
                    NotificationCategory.ApplicationInformation,
                    "Database", "Creating new omnicore database");
                await InitializeDatabase(targetAccessProvider, targetVersion, cancellationToken);
                migrationInformation.Update("Database", "Created new database", TimeSpan.FromSeconds(30));
            }
            else
            {
                var repoVersion = await GetRepositoryVersion(targetAccessProvider, cancellationToken);

                if (repoVersion == null)
                {
                    migrationInformation = NotificationFunctions.CreateNotification(
                        NotificationCategory.ApplicationImportant,
                        "Database error",
                        "Failed to determine the version of existing database, a new database will be created instead.");
                    var backupPath = targetAccessProvider.DataPath + ".failedmigration";
                    if (File.Exists(backupPath))
                        File.Delete(backupPath);
                    File.Move(targetAccessProvider.DataPath, backupPath);

                    await InitializeDatabase(targetAccessProvider, targetVersion, cancellationToken);
                    migrationInformation.Update("Database", "Created new database", TimeSpan.FromSeconds(30));
                }
                else if (repoVersion != targetVersion)
                {
                    migrationInformation = NotificationFunctions.CreateNotification(
                        NotificationCategory.ApplicationInformation,
                        "Database", "Migrating database of the previously installed OmniCore version");
                    while (repoVersion != targetVersion)
                    {
                        repoVersion = await MigrateDatabase(targetAccessProvider, repoVersion, targetVersion, cancellationToken);
                    }
                    
                    migrationInformation.Update("Database", "Database migrated successfully from previous version.", null);
                }
            }
        }
        
        private async Task InitializeDatabase(IRepositoryAccessProvider targetAccessProvider, Version targetVersion, CancellationToken cancellationToken)
        {
            await ServerContainer.Get<IMigrationHistoryRepository>().WithAccessProvider(targetAccessProvider).EnsureSchemaAndDefaults(cancellationToken);
            await ServerContainer.Get<IMedicationRepository>().WithAccessProvider(targetAccessProvider).EnsureSchemaAndDefaults(cancellationToken);
            await ServerContainer.Get<IUserRepository>().WithAccessProvider(targetAccessProvider).EnsureSchemaAndDefaults(cancellationToken);
            await ServerContainer.Get<IRadioRepository>().WithAccessProvider(targetAccessProvider).EnsureSchemaAndDefaults(cancellationToken);
            await ServerContainer.Get<IPodRepository>().WithAccessProvider(targetAccessProvider).EnsureSchemaAndDefaults(cancellationToken);
            await ServerContainer.Get<IRadioEventRepository>().WithAccessProvider(targetAccessProvider).EnsureSchemaAndDefaults(cancellationToken);
            await ServerContainer.Get<ISignalStrengthRepository>().WithAccessProvider(targetAccessProvider).EnsureSchemaAndDefaults(cancellationToken);
            await ServerContainer.Get<IMedicationDeliveryRepository>().WithAccessProvider(targetAccessProvider).EnsureSchemaAndDefaults(cancellationToken);
        }

        public async Task<Version> MigrateDatabase(IRepositoryAccessProvider accessProvider, Version fromVersion, Version toVersion, CancellationToken cancellationToken)
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
                await InitializeDatabase(accessProvider, toVersion, cancellationToken);
                return toVersion;
            }

            throw new OmniCoreRepositoryException(FailureType.RepositoryGeneralError,
                $"Repository upgrade failed. don't know how to update from application version {fromVersion}");
        }

        public Task ImportRepository(IRepositoryAccessProvider sourceAccessProvider,
            IRepositoryAccessProvider targetAccessProvider, Version migrateTo, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        
        private async Task<Version> GetRepositoryVersion(IRepositoryAccessProvider accessProvider, CancellationToken cancellationToken)
        {
            using var access = await accessProvider.ForSchema(cancellationToken);
            var connection = access.Connection;

            var rc = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM sqlite_master WHERE type=? AND name=?",
                "table", "ErosMessageExchangeResult");

            if (rc > 0)
                return new Version(1,0,0,702);

            var mh = ServerContainer.Get<IMigrationHistoryRepository>();
            mh.WithDirectAccess(access);

            return await mh.GetLastMigrationVersion(cancellationToken);
        }
        
    }
}