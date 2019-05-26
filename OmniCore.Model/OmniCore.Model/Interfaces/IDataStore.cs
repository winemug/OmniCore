using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IDataStore
    {
        Task Initialize();
        Task<bool> Load(IPod pod);
        Task<bool> Save(IPod pod);
    }
}
