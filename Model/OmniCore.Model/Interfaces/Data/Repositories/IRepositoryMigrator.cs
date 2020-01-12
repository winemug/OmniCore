using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform.Common;
using SQLite;

namespace OmniCore.Model.Interfaces.Platform.Common.Data.Repositories
{
    public interface IRepositoryMigrator : IServerResolvable
    {
        Task ExecuteMigration(Version migrateTo, string path,
            CancellationToken cancellationToken);

        Task ImportRepository(string importPath, string targetPath, Version migrateTo, CancellationToken cancellationToken);
    }
}