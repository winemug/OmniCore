using OmniCore.Impl.Eros;
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Repositories
{
    public class ErosPodRequestRepository : SqliteRepository<ErosRequest>, IPodRequestRepository<ErosRequest>
    {
        public async Task<IList<ErosRequest>> GetPendingRequests(Guid podId)
        {
            var c = await GetConnection();
            return await c.Table<ErosRequest>()
                .Where(r => r.RequestStatus == RequestState.Scheduled || r.RequestStatus == RequestState.Queued)
                .OrderBy(r => r.Created)
                .ToListAsync();            
        }
    }
}
