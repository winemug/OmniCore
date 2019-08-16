using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Plugin.BluetoothLE;

namespace OmniCore.Model.Interfaces
{
    public interface IPodRepository : IDisposable
    {
        Task<IList<T>> GetActivePods<T>() where T : IPod, new();
        Task SavePod<T>(T pod) where T : IPod, new();
    }
}
