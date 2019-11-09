using OmniCore.Repository.Enums;
using OmniCore.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Repository
{
    public class PodRequestRepository : SqliteRepositoryWithUpdate<PodRequest>
    {
        public async Task<List<PodRequest>> GetPendingRequests(long podId)
        {
            var c = await GetConnection();
            return await c.Table<PodRequest>()
                .Where(r => r.Id == podId &&
                (r.RequestStatus < RequestState.TryingToCancel))
                .OrderBy(r => r.Created)
                .ToListAsync();            
        }
    }
}
