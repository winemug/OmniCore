using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Repositories;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Repository
{
    public class Repository<T> : IRepository<T> where T : class, IEntity
    {
        public IEntitySet<T> EntitySet { get; private set; }
        public Repository(EntitySet<T> entitySet)
        {
            EntitySet = entitySet;
        }

        public void Register<T1>(IRepositoryContext context)
        {
            throw new NotImplementedException();
        }

        public void Initialize<T1>(IRepositoryContext context)
        {
            throw new NotImplementedException();
        }

        public T New()
        {
            throw new NotImplementedException();
        }

        public Task Create(T entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<T> Read(long id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<T> All(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task Update(T entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task Delete(T entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
