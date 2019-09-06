using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IPodRequestRepository<T> : IRepository<T> where T : IEntity, new()
    {
        Task<IList<T>> GetRequests(Guid podId);
    }
}
