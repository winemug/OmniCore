using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Eros
{
    public class ErosPodRequestQueue
    {
        
        private ConcurrentBag<IPodRequest> RequestList;
        private IErosPod Pod;
        private ErosPodRadioSelector PodRadioSelector;

        private readonly IRepositoryService RepositoryService;
        public ErosPodRequestQueue(IRepositoryService repositoryService)
        {
            RepositoryService = repositoryService;
        }
        
        public async Task Initialize(IErosPod pod,
            ErosPodRadioSelector podRadioSelector,
            CancellationToken cancellationToken)
        {
            Pod = pod;
            PodRadioSelector = podRadioSelector;
            RequestList = new ConcurrentBag<IPodRequest>();

            // using var context = await RepositoryService.GetContextReadOnly(cancellationToken);
            // var pendingRequests = context.PodRequests.Where(pr =>
            //     !pr.IsDeleted && pr.RequestStatus == RequestState.Aborted)
            //
            // using(var pr = RepositoryProvider.Instance.PodRequestRepository)
            // {
            //     foreach(var pendingRequest in pendingRequests.OrderBy(r => r.StartEarliest ?? r.Created))
            //     {
            //         switch (pendingRequest.RequestStatus)
            //         {
            //             case RequestState.Initializing:
            //                 pendingRequest.RequestStatus = RequestState.Aborted;
            //                 await pr.CreateOrUpdate(pendingRequest);
            //                 break;
            //             case RequestState.Executing:
            //             case RequestState.TryingToCancel:
            //                 pendingRequest.RequestStatus = RequestState.AbortedWhileExecuting;
            //                 await pr.CreateOrUpdate(pendingRequest);
            //                 break;
            //             default:
            //                 await QueueRequest(pendingRequest);
            //                 break;
            //         }
            //     
            //     }
            // }
        }
        
        // public async Task<IPodTask> QueueRequest(IPodRequest request, CancellationToken cancellationToken)
        // {
        //     var utcNow = DateTimeOffset.UtcNow;
        //     using (var context = await RepositoryService.GetContextReadWrite(cancellationToken))
        //     {
        //         var podRequestEntity = 
        //         
        //     }
        //     newRequest.RequestStatus = RequestState.Queued;
        //     if (newRequest.StartEarliest.HasValue && newRequest.StartEarliest < utcNow)
        //     {
        //         newRequest.StartEarliest = null;
        //     }
        //
        //     if (!newRequest.StartLatest.HasValue)
        //     {
        //         if (newRequest.StartEarliest.HasValue)
        //             newRequest.StartLatest = newRequest.StartEarliest.Value.AddMinutes(30);
        //         else
        //             newRequest.StartLatest = utcNow.AddMinutes(30);
        //     }
        //
        //     newRequest.Created = utcNow;
        //
        //     using(var pr = RepositoryProvider.Instance.PodRequestRepository)
        //     {
        //         RequestList.Add(new ErosRequest(BackgroundTaskFactory, RadioProviders, await pr.CreateOrUpdate(newRequest)));
        //         await ProcessQueue();
        //     }
        // }
        //
        // private async Task ProcessQueue()
        // {
        //     await RequestSemaphore.WaitAsync();
        //     try
        //     {
        //         var dtNow = DateTimeOffset.UtcNow;
        //         var pendingExecution = RequestList
        //             .Where(r => r.Request.RequestStatus == RequestState.Queued || r.Request.RequestStatus == RequestState.Scheduled)
        //             .OrderBy(r => r.Request.StartEarliest ?? r.Request.Created);
        //
        //         foreach(var request in pendingExecution)
        //         {
        //             var stateBefore = request.Request.RequestStatus;
        //             if (request.Request.StartEarliest.HasValue)
        //             {
        //                 if (dtNow < request.Request.StartEarliest)
        //                     request.Request.RequestStatus = RequestState.Scheduled;
        //                 else
        //                     request.Request.RequestStatus = RequestState.Queued;
        //             }
        //
        //             if (request.Request.StartLatest.HasValue && dtNow > request.Request.StartLatest)
        //             {
        //                 request.Request.RequestStatus = RequestState.Expired;
        //             }
        //
        //             if (stateBefore != request.Request.RequestStatus)
        //             {
        //                 using(var pr = RepositoryProvider.Instance.PodRequestRepository)
        //                 await pr.CreateOrUpdate(request.Request);
        //             }
        //         }
        //
        //         ErosRequest requestCandidate = null;
        //         var nextInQueue = pendingExecution.FirstOrDefault(r => r.Request.RequestStatus == RequestState.Queued);
        //         var nextScheduled = pendingExecution.FirstOrDefault(r => r.Request.RequestStatus == RequestState.Scheduled);
        //         var activeRequest = RequestList.FirstOrDefault(r => r.IsActive().Result);
        //
        //         if (nextInQueue != null)
        //         {
        //             requestCandidate = nextInQueue;
        //             if (activeRequest != null
        //                 && await activeRequest.IsWaitingForScheduledExecution()
        //                 && await activeRequest.TryCancelScheduledWait())
        //             {
        //                 activeRequest = null;
        //             }
        //         }
        //         else if (nextScheduled != null)
        //         {
        //             requestCandidate = nextScheduled;
        //             if (activeRequest != null && await activeRequest.IsWaitingForScheduledExecution() && activeRequest.Request.Id != nextScheduled.Request.Id
        //                 && await activeRequest.TryCancelScheduledWait())
        //             {
        //                 activeRequest = null;
        //             }
        //         }
        //
        //         if (activeRequest == null && requestCandidate != null)
        //         {
        //             if (!await requestCandidate.Run())
        //             {
        //                 throw new OmniCoreWorkflowException(FailureType.SystemError, "Couldn't create background tasks, queue aborted.");
        //             }
        //         }
        //     }
        //     catch { throw; }
        //     finally
        //     {
        //         RequestSemaphore.Release();
        //     }
        // }
        //
        // private List<PodRequest> EliminateRedundantRequests(List<PodRequest> requests, RequestType type)
        // {
        //     //TODO:
        //     return requests;
        // }
        //
        // public async Task<bool> WaitForResult(PodRequest request, int timeout)
        // {
        //     return false;
        // }
        //
        // public async Task<bool> CancelRequest(PodRequest request)
        // {
        //     return false;
        // }
      
        
    }
}