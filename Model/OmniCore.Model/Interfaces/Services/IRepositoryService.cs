using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Data.Repositories;
using OmniCore.Model.Interfaces.Platform.Common.Data.Repositories;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface IRepositoryService : ICoreService
    {
        IRepositoryAccessProvider AccessProvider { get; }
        Task Import(string importPath, CancellationToken cancellationToken);
        Task Restore(string backupPath, CancellationToken cancellationToken);
        Task Backup(string backupPath, CancellationToken cancellationToken);
    }
}
