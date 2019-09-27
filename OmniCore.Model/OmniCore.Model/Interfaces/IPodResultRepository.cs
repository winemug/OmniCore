using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IPodResultRepository<T> : IRepository<T> where T : IEntity, new()
    {
    }
}
