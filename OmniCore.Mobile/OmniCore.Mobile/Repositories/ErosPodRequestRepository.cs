using OmniCore.Impl.Eros;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Mobile.Repositories
{
    public class ErosPodRequestRepository : SqliteRepository<ErosRequest>, IPodRequestRepository<ErosRequest>
    {
        public async Task<IList<ErosRequest>> GetRequests(Guid podId)
        {
            throw new NotImplementedException();
        }
    }
}
