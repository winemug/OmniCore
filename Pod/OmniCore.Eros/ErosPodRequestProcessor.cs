using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;

namespace OmniCore.Eros
{
    public class ErosPodRequestProcessor
    {
        //        private Pod Pod { get; set; }
        //
        //        private ConcurrentBag<ErosRequest> RequestList;
        //        private SemaphoreSlim RequestSemaphore = new SemaphoreSlim(1,1);
        //        private IBackgroundTaskFactory BackgroundTaskFactory;
        //        private IRadioProvider[] RadioProviders;
        //
        //        public async Task Initialize(Pod pod, IBackgroundTaskFactory backgroundTaskFactory, IRadioProvider[] radioProviders)
        //        {
        //            Pod = pod;
        //            RequestList = new ConcurrentBag<ErosRequest>();
        //            BackgroundTaskFactory = backgroundTaskFactory;
        //            RadioProviders = radioProviders;
        //
        //            using(var pr = RepositoryProvider.Instance.PodRequestRepository)
        //            {
        //                var pendingRequests = await pr.GetPendingRequests(Pod.Id.Value);
        //                foreach(var pendingRequest in pendingRequests.OrderBy(r => r.StartEarliest ?? r.Created))
        //                {
        //                    switch (pendingRequest.RequestStatus)
        //                    {
        //                        case RequestState.Initializing:
        //                            pendingRequest.RequestStatus = RequestState.Aborted;
        //                            await pr.CreateOrUpdate(pendingRequest);
        //                            break;
        //                        case RequestState.Executing:
        //                        case RequestState.TryingToCancel:
        //                            pendingRequest.RequestStatus = RequestState.AbortedWhileExecuting;
        //                            await pr.CreateOrUpdate(pendingRequest);
        //                            break;
        //                        default:
        //                            await QueueRequest(pendingRequest);
        //                            break;
        //                    }
        //                
        //                }
        //            }
        //        }
        //
        //        public async Task<List<PodRequest>> GetActiveRequests()
        //        {
        //            return RequestList.Select(rq => rq.Request).ToList();
        //        }
        //
        //        public async Task QueueRequest(PodRequest newRequest)
        //        {
        //            var utcNow = DateTimeOffset.UtcNow;
        //            newRequest.RequestStatus = RequestState.Queued;
        //            if (newRequest.StartEarliest.HasValue && newRequest.StartEarliest < utcNow)
        //            {
        //                newRequest.StartEarliest = null;
        //            }
        //
        //            if (!newRequest.StartLatest.HasValue)
        //            {
        //                if (newRequest.StartEarliest.HasValue)
        //                    newRequest.StartLatest = newRequest.StartEarliest.Value.AddMinutes(30);
        //                else
        //                    newRequest.StartLatest = utcNow.AddMinutes(30);
        //            }
        //
        //            newRequest.Created = utcNow;
        //
        //            using(var pr = RepositoryProvider.Instance.PodRequestRepository)
        //            {
        //                RequestList.Add(new ErosRequest(BackgroundTaskFactory, RadioProviders, await pr.CreateOrUpdate(newRequest)));
        //                await ProcessQueue();
        //            }
        //        }
        //
        //        private async Task ProcessQueue()
        //        {
        //            await RequestSemaphore.WaitAsync();
        //            try
        //            {
        //                var dtNow = DateTimeOffset.UtcNow;
        //                var pendingExecution = RequestList
        //                    .Where(r => r.Request.RequestStatus == RequestState.Queued || r.Request.RequestStatus == RequestState.Scheduled)
        //                    .OrderBy(r => r.Request.StartEarliest ?? r.Request.Created);
        //
        //                foreach(var request in pendingExecution)
        //                {
        //                    var stateBefore = request.Request.RequestStatus;
        //                    if (request.Request.StartEarliest.HasValue)
        //                    {
        //                        if (dtNow < request.Request.StartEarliest)
        //                            request.Request.RequestStatus = RequestState.Scheduled;
        //                        else
        //                            request.Request.RequestStatus = RequestState.Queued;
        //                    }
        //
        //                    if (request.Request.StartLatest.HasValue && dtNow > request.Request.StartLatest)
        //                    {
        //                        request.Request.RequestStatus = RequestState.Expired;
        //                    }
        //
        //                    if (stateBefore != request.Request.RequestStatus)
        //                    {
        //                        using(var pr = RepositoryProvider.Instance.PodRequestRepository)
        //                        await pr.CreateOrUpdate(request.Request);
        //                    }
        //                }
        //
        //                ErosRequest requestCandidate = null;
        //                var nextInQueue = pendingExecution.FirstOrDefault(r => r.Request.RequestStatus == RequestState.Queued);
        //                var nextScheduled = pendingExecution.FirstOrDefault(r => r.Request.RequestStatus == RequestState.Scheduled);
        //                var activeRequest = RequestList.FirstOrDefault(r => r.IsActive().Result);
        //
        //                if (nextInQueue != null)
        //                {
        //                    requestCandidate = nextInQueue;
        //                    if (activeRequest != null
        //                        && await activeRequest.IsWaitingForScheduledExecution()
        //                        && await activeRequest.TryCancelScheduledWait())
        //                    {
        //                        activeRequest = null;
        //                    }
        //                }
        //                else if (nextScheduled != null)
        //                {
        //                    requestCandidate = nextScheduled;
        //                    if (activeRequest != null && await activeRequest.IsWaitingForScheduledExecution() && activeRequest.Request.Id != nextScheduled.Request.Id
        //                        && await activeRequest.TryCancelScheduledWait())
        //                    {
        //                        activeRequest = null;
        //                    }
        //                }
        //
        //                if (activeRequest == null && requestCandidate != null)
        //                {
        //                    if (!await requestCandidate.Run())
        //                    {
        //                        throw new OmniCoreWorkflowException(FailureType.SystemError, "Couldn't create background tasks, queue aborted.");
        //                    }
        //                }
        //            }
        //            catch { throw; }
        //            finally
        //            {
        //                RequestSemaphore.Release();
        //            }
        //        }
        //
        //        private List<PodRequest> EliminateRedundantRequests(List<PodRequest> requests, RequestType type)
        //        {
        //            //TODO:
        //            return requests;
        //        }
        //
        //        public async Task<bool> WaitForResult(PodRequest request, int timeout)
        //        {
        //            return false;
        //        }
        //
        //        public async Task<bool> CancelRequest(PodRequest request)
        //        {
        //            return false;
        //        }
        //
        //
    }
}
