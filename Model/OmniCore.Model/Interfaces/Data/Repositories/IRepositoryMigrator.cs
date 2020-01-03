using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform;
using SQLite;

namespace OmniCore.Model.Interfaces.Data.Repositories
{
    public interface IRepositoryMigrator : IServerResolvable
    {
        Task ExecuteMigration(Version migrateTo, string path,
            CancellationToken cancellationToken);

        Task ImportRepository(string importPath, string targetPath, Version migrateTo, CancellationToken cancellationToken);
    }
}