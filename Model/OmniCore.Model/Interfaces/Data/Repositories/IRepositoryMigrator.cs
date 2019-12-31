using System;
using System.Threading;
using System.Threading.Tasks;
using SQLite;

namespace OmniCore.Model.Interfaces.Data.Repositories
{
    public interface IRepositoryMigrator
    {
        Task ExecuteMigration(Version migrateTo, string path,
            CancellationToken cancellationToken);

        Task ImportRepository(string importPath, string targetPath, Version migrateTo, CancellationToken cancellationToken);
    }
}