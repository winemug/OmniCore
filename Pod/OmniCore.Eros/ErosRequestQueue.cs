using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Eros
{
    public class ErosRequestQueue : IServerResolvable
    {
        private readonly BlockingCollection<ErosPodRequest> RequestQueue;

        private readonly ISubject<ErosPodRequest> RequestSubject;

        public ErosRequestQueue()
        {
            RequestQueue = new BlockingCollection<ErosPodRequest>(new ConcurrentQueue<ErosPodRequest>());
        }

        public void Startup()
        {
            // TODO: load here
        }

        public void Shutdown()
        {
            RequestQueue.CompleteAdding();
        }

        public ErosPodRequest Enqueue(ErosPodRequest request)
        {
            if (RequestQueue.IsAddingCompleted)
                throw new OmniCoreWorkflowException(FailureType.Internal,
                    "Queue is shutting down, no new jobs can be added.");

            RequestQueue.Add(request);
            return request;
        }

        private async Task ConsumeQueue()
        {
            while (!RequestQueue.IsCompleted)
                if (RequestQueue.TryTake(out var request))
                    await request.ExecuteRequest();
        }
    }
}