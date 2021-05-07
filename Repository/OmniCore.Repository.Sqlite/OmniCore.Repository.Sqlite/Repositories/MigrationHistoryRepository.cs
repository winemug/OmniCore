using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using OmniCore.Model.Interfaces.Platform.Common.Data.Repositories;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Repository.Sqlite.Entities;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class MigrationHistoryRepository : Repository<MigrationHistoryEntity, IMigrationHistoryEntity>, IMigrationHistoryRepository
    {
        private Version CurrentVersion;
        public MigrationHistoryRepository(IRepositoryService repositoryService,
            ICoreApplicationFunctions coreApplicationFunctions) : base(repositoryService)
        {
            CurrentVersion = coreApplicationFunctions.Version;
        }

        public override async Task EnsureSchemaAndDefaults(CancellationToken cancellationToken)
        {
            await base.EnsureSchemaAndDefaults(cancellationToken);
            var lastVersion = await GetLastMigrationVersion(cancellationToken);
            if (lastVersion == null)
            {
                await DataTask(c =>
                {
                    var mhe = New();
                    mhe.ImportPath = c.DatabasePath;
                    mhe.ToMajor = CurrentVersion.Major;
                    mhe.ToMinor = CurrentVersion.Minor;
                    mhe.ToBuild = CurrentVersion.Build;
                    mhe.ToRevision = CurrentVersion.Revision;
                    return c.InsertAsync(mhe, typeof(MigrationHistoryEntity));
                }, cancellationToken);
            }
        }

        public Task<Version> GetLastMigrationVersion(CancellationToken cancellationToken)
        {
            return DataTask(async (c) =>
            {
                var mhe = await c.Table<MigrationHistoryEntity>()
                    .OrderByDescending(mh => mh.Created)
                    .FirstOrDefaultAsync();

                if (mhe == null)
                    return null;
                return new Version(mhe.ToMajor, mhe.ToMinor, mhe.ToBuild, mhe.ToRevision);
            }, cancellationToken);
        }
    }
}