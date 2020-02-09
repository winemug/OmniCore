using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Base;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IRepositoryService : ICoreService
    {
        Task Import(string importPath, CancellationToken cancellationToken);
        Task Restore(string backupPath, CancellationToken cancellationToken);
        Task Backup(string backupPath, CancellationToken cancellationToken);
    }
}
