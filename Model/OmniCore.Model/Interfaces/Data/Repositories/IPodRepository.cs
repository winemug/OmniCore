using System.Collections.Generic;
using OmniCore.Model.Interfaces.Common.Data.Entities;

namespace OmniCore.Model.Interfaces.Common.Data.Repositories
{
    public interface IPodRepository : IRepository<IPodEntity>
    {
        IAsyncEnumerable<IPodEntity> ActivePods();
        IAsyncEnumerable<IPodEntity> ArchivedPods();
    }
}
