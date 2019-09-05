using OmniCore.Impl.Eros;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Mobile.Repositories
{
    public class ErosPodRequestRepository : SqliteRepository, IPodRequestRepository<ErosRequest>
    {
        public async Task DeleteRequest(ErosRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<ErosRequest>> GetRequests(Guid podId)
        {
            throw new NotImplementedException();
        }

        public async Task SaveRequest(ErosRequest request)
        {
            var c = await GetConnection();
            if (request.Id == Guid.Empty)
            {
                request.Id = Guid.NewGuid();
                request.Created = DateTimeOffset.UtcNow;
            }
            await c.InsertOrReplaceAsync(request);
        }
    }
}
