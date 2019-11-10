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
        Task<Pod> New(UserProfile up, List<Radio> radios);
        Task<Pod> Register(Pod pod, UserProfile up, List<Radio> radios);
        Task QueueRequest(PodRequest request);
        Task<bool> WaitForResult(PodRequest request, int timeout);
        Task<bool> CancelRequest(PodRequest request);
        Task<List<PodRequest>> GetActiveRequests(Pod pod);
        Task<List<PodRequest>> GetActiveRequests();
        IObservable<Radio> ListRadios();
    }
}
