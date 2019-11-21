using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Repositories;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Repository.Sqlite.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class GenericRepository<T, U> : Repository, IGenericRepository<U> where U : IEntity
    {

        public GenericRepository(IRepositoryService repositoryService) : base(repositoryService)
        {
        }

        public Task Create(U entity)
        {
            throw new NotImplementedException();
        }

        public Task Delete(U entity)
        {
            throw new NotImplementedException();
        }

        public Task Hide(U entity)
        {
            throw new NotImplementedException();
        }

        public Task<U> Read(long id)
        {
            throw new NotImplementedException();
        }

        public Task<IAsyncEnumerable<U>> ReadAll()
        {
            throw new NotImplementedException();
        }

        public Task Unhide(U entity)
        {
            throw new NotImplementedException();
        }

        public Task Update(U entity)
        {
            throw new NotImplementedException();
        }
    }
}
