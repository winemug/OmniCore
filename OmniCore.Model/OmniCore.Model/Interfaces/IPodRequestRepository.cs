using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IPodRequestRepository<T> : IRepository where T : IPodRequest
    {
        Task SaveRequest(T request);
        Task DeleteRequest(T request);
        Task<IList<T>> GetRequests(Guid podId);
    }
}
