using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ICoreRepositoryService : ICoreService
    {
        Task Import(string importPath, CancellationToken cancellationToken);
        Task Restore(string backupPath, CancellationToken cancellationToken);
        Task Backup(string backupPath, CancellationToken cancellationToken);

        Task<IRepositoryContext> GetReaderContext(CancellationToken cancellationToken);
        Task<IRepositoryContextWriteable> GetWriterContext(CancellationToken cancellationToken);
    }
}
