using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IRepositoryContextReadWrite : IRepositoryContextReadOnly
    {
        Task InitializeDatabase(CancellationToken cancellationToken, bool createNew = false);
        Task Save(CancellationToken cancellationToken);
        IRepositoryContextReadWrite WithExisting(params IEntity[] entities);
        IRepositoryContextReadWrite WithExisting<T>(ICollection<T> entities)
            where T : IEntity;
        
    }
}