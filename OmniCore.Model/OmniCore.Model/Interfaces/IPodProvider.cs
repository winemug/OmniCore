using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IPodProvider
    {
        Task Initialize();

        IPodManager PodManager { get; }
        Task Archive();
        Task<IPodManager> New();
        Task<IPodManager> Register(uint lot, uint serial, uint radioAddress);
        event EventHandler ManagerChanged;
    }
}
