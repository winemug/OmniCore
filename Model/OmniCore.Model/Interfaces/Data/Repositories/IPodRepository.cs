using System.Collections.Generic;
using OmniCore.Model.Interfaces.Data.Entities;

namespace OmniCore.Model.Interfaces.Data.Repositories
{
    public interface IPodRepository : IRepository<IPodEntity>
    {
        IAsyncEnumerable<IPodEntity> ActivePods();
        IAsyncEnumerable<IPodEntity> ArchivedPods();
    }
}
