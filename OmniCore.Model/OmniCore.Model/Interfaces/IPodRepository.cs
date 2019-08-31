using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Plugin.BluetoothLE;

namespace OmniCore.Model.Interfaces
{
    public interface IPodRepository<T> : IRepository where T : IPod, new()
    {
        Task<IList<T>> GetActivePods();
        Task SavePod(T pod);
    }
}
