using OmniCore.Model.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Repositories
{
    public interface IGenericRepository<T> : IBasicRepository<T> where T : IEntity
    { 
        Task Update(T entity);
        Task Hide(T entity);
        Task Restore(T entity);
        Task Delete(T entity);
    }
}
