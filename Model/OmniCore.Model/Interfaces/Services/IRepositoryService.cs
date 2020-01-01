using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Data.Repositories;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IRepositoryService : ICoreService
    {
        Task<IRepositoryAccess> GetAccess(CancellationToken cancellationToken);
        string RepositoryPath { get; }
        Task Import(string importPath, CancellationToken cancellationToken);
        Task Restore(string backupPath, CancellationToken cancellationToken);
        Task Backup(string backupPath, CancellationToken cancellationToken);
    }
}
