using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Data.Repositories;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using SQLite;

namespace OmniCore.Model.Interfaces.Platform.Common.Data.Repositories
{
    public interface IRepository<T> : IServerResolvable
    {
        T New();
        Task Create(T entity, CancellationToken cancellationToken);
        Task<T> Read(long id, CancellationToken cancellationToken);
        Task<IList<T>> All(CancellationToken cancellationToken);
        Task Update(T entity, CancellationToken cancellationToken);
        Task Delete(T entity, CancellationToken cancellationToken);
        Task EnsureSchemaAndDefaults(CancellationToken cancellationToken);
        IRepository<T> WithAccessProvider(IRepositoryAccessProvider accessProvider);
        IRepository<T> WithDirectAccess(IRepositoryAccess access);
    }
}
