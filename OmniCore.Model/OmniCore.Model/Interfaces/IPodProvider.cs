using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Enums;

namespace OmniCore.Model.Interfaces
{
    public interface IPodProvider
    {
        Task<IPod> GetActivePod();
        Task<IEnumerable<IPod>> GetActivePods();
        Task Archive(IPod pod);
        Task<IPod> New(IEnumerable<IRadio> radios);
        Task<IPod> Register(uint lot, uint serial, uint radioAddress, IEnumerable<IRadio> radios);
        Task CancelConversations(IPod pod);
        IObservable<IRadio> ListAllRadios();
    }
}
