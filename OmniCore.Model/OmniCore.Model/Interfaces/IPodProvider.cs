using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Enums;

namespace OmniCore.Model.Interfaces
{
    public interface IPodProvider<T> where T : IPod, new()
    {
        Task<T> GetActivePod();
        Task<IEnumerable<T>> GetActivePods();
        Task Archive(T pod);
        Task<T> New(IEnumerable<IRadio> radios);
        Task<T> Register(uint lot, uint serial, uint radioAddress, IEnumerable<IRadio> radios);
        Task CancelConversations(T pod);
        IObservable<IRadio> ListAllRadios();
    }
}
