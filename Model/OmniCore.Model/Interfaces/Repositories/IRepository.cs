using OmniCore.Model.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Repositories
{
    public interface IRepository<T> where T : IEntity
    {
        IExtendedAttributeProvider ExtendedAttributeProvider { get; set; }
        T New();
        Task Create(T entity);
        Task<T> Read(long id);
        IAsyncEnumerable<T> All();
        Task Update(T entity);
        Task Hide(T entity);
        Task Restore(T entity);
        Task Delete(T entity);
    }
}
