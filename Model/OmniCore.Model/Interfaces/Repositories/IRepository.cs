using OmniCore.Model.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SQLite;

namespace OmniCore.Model.Interfaces.Repositories
{
    public interface IRepository<T> : IRepositoryInitialization where T : IEntity
    {
        IExtendedAttributeProvider ExtendedAttributeProvider { get; set; }
        T New();
        Task Create(T entity, CancellationToken cancellationToken);
        Task<T> Read(long id, CancellationToken cancellationToken);
        IAsyncEnumerable<T> All(CancellationToken cancellationToken);
        Task Update(T entity, CancellationToken cancellationToken);
        Task Delete(T entity, CancellationToken cancellationToken);
    }
}
