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
        Task<T> Register(T pod, IEnumerable<IRadio> radios);
        Task QueueRequest(IPodRequest<T> request);
        Task<bool> WaitForResult(IPodRequest<T> request, int timeout);
        Task<bool> CancelRequest(IPodRequest<T> request);
        Task<IList<IPodRequest<T>>> GetActiveRequests(T pod);
        Task<IList<IPodRequest<T>>> GetActiveRequests();
        IObservable<IRadio> ListAllRadios();
    }
}
