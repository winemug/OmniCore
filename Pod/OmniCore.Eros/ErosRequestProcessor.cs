using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace OmniCore.Eros
{
    public class ErosRequestProcessor
    {
        private IPodRequestRepository<ErosRequest> RequestRepository { get; set;}
        private ErosPod Pod { get; set; }

        private ConcurrentBag<ErosRequest> RequestList;
        private SemaphoreSlim RequestSemaphore = new SemaphoreSlim(1,1);
        private IBackgroundTask RequestTask = null;
        private IBackgroundTask SchedulerTask = null;

        public async Task Initialize(ErosPod pod, IPodRequestRepository<ErosRequest> requestRepository)
        {
            RequestRepository = requestRepository;
            Pod = pod;
            RequestTask = null;
            SchedulerTask = null;
            RequestList = new ConcurrentBag<ErosRequest>();

            var pendingRequests = await RequestRepository.GetPendingRequests(Pod.Id);
            foreach(var pendingRequest in pendingRequests.OrderBy(r => r.StartEarliest ?? r.Created))
            {
                switch (pendingRequest.RequestStatus)
                {
                    case RequestState.Initializing:
                        pendingRequest.RequestStatus = RequestState.Aborted;
                        await RequestRepository.CreateOrUpdate(pendingRequest);
                        break;
                    case RequestState.Executing:
                    case RequestState.TryCancelling:
                        pendingRequest.RequestStatus = RequestState.AbortedWhileExecuting;
                        await RequestRepository.CreateOrUpdate(pendingRequest);
                        break;
                    default:
                        await QueueRequest(pendingRequest);
                        break;
                }
                
            }
        }

        public async Task<List<ErosRequest>> GetActiveRequests()
        {
            return RequestList.ToList();
        }

        public async Task QueueRequest(ErosRequest newRequest)
        {
            var utcNow = DateTimeOffset.UtcNow;
            newRequest.RequestStatus = RequestState.Queued;
            if (newRequest.StartEarliest.HasValue && newRequest.StartEarliest < utcNow)
            {
                newRequest.StartEarliest = null;
            }

            if (!newRequest.StartLatest.HasValue)
            {
                newRequest.StartLatest = DateTimeOffset.UtcNow.AddMinutes(30);
            }

            newRequest.Created = utcNow;

            RequestList.Add(await RequestRepository.CreateOrUpdate(newRequest));
            await ProcessQueue();
        }

        private async Task ProcessQueue()
        {
            var dtNow = DateTimeOffset.UtcNow;
            var pendingExecution = RequestList
                .Where(r => r.RequestStatus == RequestState.Queued || r.RequestStatus == RequestState.Scheduled)
                .OrderBy(r => r.Created);

            foreach(var request in pendingExecution)
            {
                var stateBefore = request.RequestStatus;
                if (request.StartEarliest.HasValue)
                {
                    if (dtNow < request.StartEarliest)
                        request.RequestStatus = RequestState.Scheduled;
                    else
                        request.RequestStatus = RequestState.Queued;
                }

                if (request.StartLatest.HasValue && dtNow > request.StartLatest)
                {
                    request.RequestStatus = RequestState.Expired;
                }

                if (stateBefore != request.RequestStatus)
                    await RequestRepository.CreateOrUpdate(request);
            }

            await RequestSemaphore.WaitAsync();
            try
            {
                var activeRequest = RequestList.SingleOrDefault(r => r.RequestStatus >= RequestState.Initializing && r.RequestStatus <= RequestState.TryCancelling);
                if (activeRequest == null)
                {
                    var nextInQueue = pendingExecution.FirstOrDefault(r => r.RequestStatus == RequestState.Queued);

                    if (nextInQueue != null)
                    {

                    }
                    else
                    {
                        var nextScheduled = pendingExecution.FirstOrDefault(r => r.RequestStatus == RequestState.Scheduled);
                        if (nextScheduled != null)
                        {

                        }
                    }
                }
            }
            catch { throw; }
            finally
            {
                RequestSemaphore.Release();
            }
        }

        private List<ErosRequest> EliminateRedundantRequests(List<ErosRequest> requests, RequestType type)
        {
            //TODO:
            return requests;
        }

        public async Task<bool> WaitForResult(ErosRequest request, int timeout)
        {
            return false;
        }

        public async Task<bool> CancelRequest(ErosRequest request)
        {
            return false;
        }
    }
}
