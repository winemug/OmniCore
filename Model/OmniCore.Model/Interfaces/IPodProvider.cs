using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Repository.Entities;
using OmniCore.Repository.Enums;

namespace OmniCore.Model.Interfaces
{
    public interface IPodProvider
    {
        Task<Pod> GetActivePod();
        Task<List<Pod>> GetActivePods();
        Task Archive(Pod pod);
        Task<Pod> New(List<IRadio> radios);
        Task<Pod> Register(Pod pod, List<IRadio> radios);
        Task QueueRequest(PodRequest request);
        Task<bool> WaitForResult(PodRequest request, int timeout);
        Task<bool> CancelRequest(PodRequest request);
        Task<List<PodRequest>> GetActiveRequests(Pod pod);
        Task<List<PodRequest>> GetActiveRequests();
        IObservable<IRadio> ListAllRadios();
    }
}
