using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Entities;

namespace OmniCore.Model.Interfaces.Repositories
{
    public interface IBasicRepository<T> where T : IBasicEntity
    {
        Task<T> New();
        Task Create(T entity);
        Task<T> Read(long id);
        IAsyncEnumerable<T> All();
    }

}
