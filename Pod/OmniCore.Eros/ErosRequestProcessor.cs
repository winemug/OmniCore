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

            var pendingRequests = await GetPendingRequests();
            foreach(var pendingRequest in pendingRequests.OrderBy(r => r.StartEarliest ?? r.Created))
            {
                await QueueRequest(pendingRequest);
            }
        }


        public async Task QueueRequest(ErosRequest newRequest)
        {
            await queueSemaphore.WaitAsync();
            try
            {
                if (newRequest != null)
                {
                    newRequest.RequestStatus = RequestState.Queued;
                    RequestList.Add(newRequest);
                }

                var dtNow = DateTimeOffset.UtcNow;
                var filteredList = RequestList
                    .Where(r => r.RequestStatus == RequestState.Queued || r.RequestStatus == RequestState.Scheduled)
                    .OrderBy(r => r.StartEarliest ?? r.Created).ToList();

                for (int i = 0; i < filteredList.Count; i++)
                {
                    var request = RequestList[i];
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

                    RequestList[i] = await RequestRepository.CreateOrUpdate(request);
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
            finally
            {
                queueSemaphore.Release();
            }
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
