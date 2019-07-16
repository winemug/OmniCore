using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IPodProvider
    {
        Task Initialize();
        IEnumerable<IPod> Pods { get; }
        Task Archive(IPod pod);
        Task<IPod> New();
        Task<IPod> Register(uint lot, uint serial, uint radioAddress);
        IPod SinglePod { get; }
    }
}
