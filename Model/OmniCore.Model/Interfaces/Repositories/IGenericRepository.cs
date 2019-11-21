using OmniCore.Model.Interfaces.Attributes;
using OmniCore.Model.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : IEntity
    { 
        Task Create(T entity);
        Task<T> Read(long id);
        Task<IAsyncEnumerable<T>> ReadAll();
        Task Update(T entity);
        Task Hide(T entity);
        Task Unhide(T entity);
        Task Delete(T entity);
    }
}
