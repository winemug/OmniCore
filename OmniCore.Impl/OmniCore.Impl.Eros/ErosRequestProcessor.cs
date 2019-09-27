using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace OmniCore.Impl.Eros
{
    public class ErosRequestProcessor
    {
        private IPodRequestRepository<ErosRequest> RequestRepository { get; set;}
        private IPodResultRepository<ErosResult> ResultRepository { get; set;}
        private ErosPod Pod { get; set; }

        private ConcurrentQueue<ErosRequest> RequestQueue {get; set;}

        private Task RequestTask {get; set;}

        public async Task Initialize(ErosPod pod, IPodRequestRepository<ErosRequest> requestRepository,
            IPodResultRepository<ErosResult> resultRepository)
        {
            RequestRepository = requestRepository;
            ResultRepository = resultRepository;
            Pod = pod;

            var pendingRequests = await GetPendingRequests();
            foreach(var pendingRequest in pendingRequests)
            {
                await QueueRequest(pendingRequest);
            }
        }


        public async Task QueueRequest(ErosRequest request)
        {
            request = await RequestRepository.CreateOrUpdate(request);
            RequestQueue.Enqueue(request);
            ExecuteNext();
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
            return (await RequestRepository.GetPendingRequests(Pod.Id))
                .OrderBy(r => r.StartEarliest ?? r.Created).ToList();
        }

        private void ExecuteNext()
        {
            if (RequestTask == null)
            {

            }
        }

        private async Task ProcessRequest(ErosRequest request)
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

    }
}
