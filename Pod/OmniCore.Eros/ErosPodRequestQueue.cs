using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Eros
{
    public class ErosPodRequestQueue
    {
        
        private ConcurrentBag<IPodRequest> RequestList;
        private IErosPod Pod;
        private ErosPodRadioSelector PodRadioSelector;

        private readonly IRepositoryService RepositoryService;
        private readonly IErosRadioProvider[] ErosRadioProviders;
        private readonly IContainer Container;
        private Task ActiveRequestTask;
        public ErosPodRequestQueue(IRepositoryService repositoryService,
            IErosRadioProvider[] radioProviders,
            IContainer container)
        {
            RepositoryService = repositoryService;
            ErosRadioProviders = radioProviders;
            Container = container;
        }

        public async Task Initialize(IErosPod pod,
            CancellationToken cancellationToken)
        {
            Pod = pod;
            
            var radios = new List<IErosRadio>();
            Pod.Entity
                .PodRadios
                .Select(pr => pr.Radio)
                .ToList()
                .ForEach(async r =>
                {
                    radios.Add(await 
                        ErosRadioProviders.Single(rp => rp.ServiceUuid == r.ServiceUuid)
                            .GetRadio(r.DeviceUuid, cancellationToken));
                });

            PodRadioSelector = await Container.Get<ErosPodRadioSelector>();
            await PodRadioSelector.Initialize(radios);

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

        public void Shutdown()
        {
            
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
      
//                 private async Task StartProbing(CancellationToken cancellationToken)
//         {
//             await StartProbing(Entity.Options.StatusCheckIntervalGood, cancellationToken);
//         }
//         
//         private async Task StartProbing(TimeSpan initialProbe, CancellationToken cancellationToken)
//         {
//             if (!Entity.IsDeleted && Entity.PodRadios.Count > 0)
//             {
//                 await ScheduleProbe(TimeSpan.FromSeconds(10), cancellationToken);
//             }
//         }
//         private async Task ScheduleProbe(TimeSpan interval, CancellationToken cancellationToken)
//         {
//             using var _ = await ProbeStartStopLock.LockAsync(cancellationToken);
//             StatusCheckCancellationTokenSource?.Dispose();
//             StatusCheckCancellationTokenSource = new CancellationTokenSource();
//
//             StatusCheckSubscription?.Dispose();
//             StatusCheckSubscription = NewThreadScheduler.Default.Schedule(
//                 interval,
//                 async () =>
//                 {
//                     var nextInterval = Entity.Options.StatusCheckIntervalGood;
//                     try
//                     {
//                         nextInterval = await PerformProbe(StatusCheckCancellationTokenSource.Token);
//                     }
//                     catch (Exception e)
//                     {
//                         if (StatusCheckCancellationTokenSource.IsCancellationRequested)
//                         {
//                             Logger<>.Information($"Pod probe canceled");
//                         }
//                         else
//                         {
//                             Logger<>.Warning($"Pod probe failed", e);
//                             nextInterval = Entity.Options.StatusCheckIntervalBad;
//                         }
//                     }
// #if DEBUG
//                     nextInterval = TimeSpan.FromSeconds(10);
// #endif
//
//                     await ScheduleProbe(nextInterval, StatusCheckCancellationTokenSource.Token);
//                 });
//         }
//
//         private void StopProbing(CancellationToken cancellationToken)
//         {
//             using var _ = ProbeStartStopLock.Lock(cancellationToken);
//             StatusCheckSubscription?.Dispose();
//             StatusCheckSubscription = null;
//
//             if (StatusCheckCancellationTokenSource != null)
//             {
//                 StatusCheckCancellationTokenSource.Cancel();
//                 StatusCheckCancellationTokenSource.Dispose();
//                 StatusCheckCancellationTokenSource = null;
//             }
//         }
//
//         private async Task<TimeSpan> PerformProbe(CancellationToken cancellationToken)
//         {
//             Logger<>.Information("Starting pod probe");
//
//             var radio = await RadioSelector.Select(cancellationToken);
//             await radio.PerformHealthCheck(cancellationToken);
//             
//             Logger<>.Information("Pod probe ended");
//             return Entity.Options.StatusCheckIntervalGood;
//         }

        public async Task<ErosPodRequest> Enqueue(ErosPodRequest erosPodRequest)
        {
            //TODO:
            return await StartExecute(erosPodRequest);
        }

        private async Task<ErosPodRequest> StartExecute(ErosPodRequest request)
        {
            var radio = await PodRadioSelector.Select(request.CancellationToken);
            ActiveRequestTask = Task.Run(async () => await request.ExecuteRequest(radio));
            return request;
        }
    }
}