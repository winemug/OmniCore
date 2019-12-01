using OmniCore.Model.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Repositories
{
    public interface IPodRepository : IRepository<IPodEntity>
    {
        Task<IList<IPodEntity>> ActivePods();
        Task<IList<IPodEntity>> ArchivedPods();
    }
}
