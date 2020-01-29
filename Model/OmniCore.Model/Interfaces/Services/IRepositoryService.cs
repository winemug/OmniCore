using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Data;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface IRepositoryService : ICoreService
    {
        Task Import(string importPath, CancellationToken cancellationToken);
        Task Restore(string backupPath, CancellationToken cancellationToken);
        Task Backup(string backupPath, CancellationToken cancellationToken);
    }
}
