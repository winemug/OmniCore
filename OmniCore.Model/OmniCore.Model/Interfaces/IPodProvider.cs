using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IPodProvider
    {
        Task<IPod> GetActivePod();
        Task<IEnumerable<IPod>> GetActivePods();
        Task Archive(IPod pod);
        Task<IPod> New();
        Task<IPod> Register(uint lot, uint serial, uint radioAddress);
        Task<IConversation> StartConversation(IPod pod);
        Task CancelConversations(IPod pod);
    }
}
