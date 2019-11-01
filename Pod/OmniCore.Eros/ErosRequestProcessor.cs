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
        private IPodResultRepository<ErosResult> ResultRepository { get; set;}
        private ErosPod Pod { get; set; }

        private List<ErosRequest> RequestList {get; set;}
        private SemaphoreSlim queueSemaphore = new SemaphoreSlim(1,1);
        private ErosRequest ActiveRequest { get; set; }

        public async Task Initialize(ErosPod pod, IPodRequestRepository<ErosRequest> requestRepository,
            IPodResultRepository<ErosResult> resultRepository)
        {
            RequestRepository = requestRepository;
            ResultRepository = resultRepository;
            Pod = pod;
            ActiveRequest = null;
            RequestList = new List<ErosRequest>();

            var pendingRequests = await GetPendingRequests();
            foreach(var pendingRequest in pendingRequests.OrderBy(r => r.StartEarliest ?? r.Created))
            {
                switch (pendingRequest.RequestStatus)
                {
                    case RequestState.Initializing:
                        pendingRequest.RequestStatus = RequestState.Aborted;
                        await RequestRepository.CreateOrUpdate(pendingRequest);
                        break;
                    case RequestState.Executing:
                    case RequestState.TryCancel:
                        pendingRequest.RequestStatus = RequestState.AbortedWhileExecuting;
                        await RequestRepository.CreateOrUpdate(pendingRequest);
                        break;
                    default:
                        await QueueRequest(pendingRequest);
                        break;
                }
                
            }
        }


        public async Task QueueRequest(ErosRequest newRequest)
        {
            await queueSemaphore.WaitAsync();
            try
            {
                newRequest.RequestStatus = RequestState.Queued;
                if (!newRequest.StartLatest.HasValue)
                {
                    newRequest.StartLatest = DateTimeOffset.UtcNow.AddMinutes(30);
                }
                RequestList.Add(await RequestRepository.CreateOrUpdate(newRequest));
                await ProcessQueue();
            }
            finally
            {
                queueSemaphore.Release();
            }
        }

        private async Task ProcessQueue()
        {
            var dtNow = DateTimeOffset.UtcNow;
            var filteredList = RequestList
                .Where(r => r.RequestStatus == RequestState.Queued || r.RequestStatus == RequestState.Scheduled);

            foreach(var request in filteredList)
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

            filteredList = filteredList.Where(r => r.RequestStatus != RequestState.Expired).ToList();

            var queuedList = filteredList
                .Where(r => r.RequestStatus == RequestState.Queued)
                .OrderBy(r => r.Created)
                .ToList();

            var scheduledList = filteredList.Where(r => r.RequestStatus == RequestState.Scheduled)
                .Where(r => r.RequestStatus == RequestState.Scheduled)
                .OrderBy(r => r.StartEarliest)
                .ToList();
        }

        private List<ErosRequest> EliminateRedundantRequests(List<ErosRequest> requests, RequestType type)
        {
            //TODO:
            return requests;
        }

        public async Task<ErosResult> GetResult(ErosRequest request, int timeout)
        {
            try
            {
                return new ErosResult() { ResultType = ResultType.OK };
            }
            catch (OperationCanceledException oce)
            {
                return new ErosResult() { ResultType = ResultType.Canceled, Exception = oce };
            }
            catch (Exception e)
            {
                return new ErosResult() { ResultType = ResultType.Error, Exception = e };
            }
        }

        public async Task<bool> CancelRequest(ErosRequest request)
        {
            return false;
        }

        public async Task<List<ErosRequest>> GetPendingRequests()
        {
            return (await RequestRepository.GetPendingRequests(Pod.Id)).ToList();
        }

        private async Task<ErosRequest> GetNextRequest()
        {
            ErosRequest nextRequest = null;
            return nextRequest;
        }


        private async Task ProcessRequest(ErosRequest request)
        {
        }
    }
}
