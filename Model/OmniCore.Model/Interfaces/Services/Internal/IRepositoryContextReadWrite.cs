using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IRepositoryContextReadWrite : IRepositoryContextReadOnly
    {
        Task Save(CancellationToken cancellationToken);
        IRepositoryContextReadWrite WithExisting(Entity entity);
    }
}