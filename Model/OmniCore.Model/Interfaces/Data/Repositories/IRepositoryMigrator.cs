using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Data.Repositories;
using OmniCore.Model.Interfaces.Platform.Common;
using SQLite;

namespace OmniCore.Model.Interfaces.Platform.Common.Data.Repositories
{
    public interface IRepositoryMigrator : IServerResolvable
    {
        Task ExecuteMigration(Version targetVersion, IRepositoryAccessProvider targetAccessProvider,
            CancellationToken cancellationToken);

        Task ImportRepository(IRepositoryAccessProvider sourceAccessProvider,
            IRepositoryAccessProvider targetAccessProvider, Version migrateTo, CancellationToken cancellationToken);
    }
}