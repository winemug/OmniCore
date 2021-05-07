using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IRepositoryService : IService
    {
        Task Import(string importPath, CancellationToken cancellationToken);
        Task Restore(string backupPath, CancellationToken cancellationToken);
        Task Backup(string backupPath, CancellationToken cancellationToken);

        Task<IRepositoryContextReadOnly> GetContextReadOnly(CancellationToken cancellationToken);
        Task<IRepositoryContextReadWrite> GetContextReadWrite(CancellationToken cancellationToken);
    }
}