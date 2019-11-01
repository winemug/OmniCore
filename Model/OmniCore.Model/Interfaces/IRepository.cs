using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IRepository<T> : IDisposable where T : IEntity, new()
    {
        Task Initialize();
        Task<T> CreateOrUpdate(T entity);
        Task<T> Read(Guid entityId);
        Task Delete(Guid entityId);
    }
}
