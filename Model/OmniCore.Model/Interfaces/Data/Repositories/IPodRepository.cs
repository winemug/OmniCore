using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;

namespace OmniCore.Model.Interfaces.Platform.Common.Data.Repositories
{
    public interface IPodRepository : IRepository<IPodEntity>
    {
        Task<IList<IPodEntity>> ActivePods(CancellationToken cancellationToken);
        //Task<IList<IPodEntity>> ArchivedPods(CancellationToken cancellationToken);
        Task<IPodEntity> ByLotAndSerialNo(uint lot, uint serial, CancellationToken cancellationToken);
    }
}
