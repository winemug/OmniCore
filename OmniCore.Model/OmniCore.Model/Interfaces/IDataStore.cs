using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IDataStore
    {
        void Initialize();
        bool Load(IPod pod);
        void Save(IPod pod);
    }
}
