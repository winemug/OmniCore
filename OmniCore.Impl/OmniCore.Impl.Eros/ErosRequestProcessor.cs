using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Impl.Eros
{
    public class ErosRequestProcessor
    {
        private List<ErosRequest> Request { get; }
        public Dictionary<Guid,CancellationTokenSource> CancellationSources { get; }
        public Dictionary<Guid, TaskCompletionSource<ErosResult>> ResultSources { get; }

        private IPodRequestRepository<ErosRequest> RequestRepository;
        private ErosPod Pod;

        public ErosRequestProcessor(ErosPod pod, IPodRequestRepository<ErosRequest> requestRepository)
        {
            RequestRepository = requestRepository;
            Pod = pod;
            CancellationSources = new Dictionary<Guid, CancellationTokenSource>();
            ResultSources = new Dictionary<Guid, TaskCompletionSource<ErosResult>>();
        }

        public async Task QueueRequest(ErosRequest request)
        {
            request = await RequestRepository.CreateOrUpdate(request);
            CancellationSources.Add(request.Id, new CancellationTokenSource());
            ResultSources.Add(request.Id, new TaskCompletionSource<ErosResult>());
        }

        public async Task<ErosResult> GetResult(ErosRequest request, int timeout)
        {
            try
            {
                return new ErosResult(ResultType.OK);
            }
            catch (OperationCanceledException oce)
            {
                return new ErosResult(ResultType.Canceled, oce);
            }
            catch (Exception e)
            {
                return new ErosResult(ResultType.Error, e);
            }
        }

        public async Task<bool> CancelRequest(ErosRequest request)
        {
            return false;
        }

        public async Task<List<ErosRequest>> GetPendingRequests()
        {
            throw new NotImplementedException();
        }
    }
}
