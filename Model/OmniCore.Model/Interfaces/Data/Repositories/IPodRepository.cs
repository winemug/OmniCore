using System.Collections.Generic;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;

namespace OmniCore.Model.Interfaces.Platform.Common.Data.Repositories
{
    public interface IPodRepository : IRepository<IPodEntity>
    {
        IAsyncEnumerable<IPodEntity> ActivePods();
        IAsyncEnumerable<IPodEntity> ArchivedPods();
    }
}
