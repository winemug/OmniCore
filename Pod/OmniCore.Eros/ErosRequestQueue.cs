using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services.Facade;

namespace OmniCore.Eros
{
    public class ErosRequestQueue 
    {
        private readonly BlockingCollection<IErosPodRequest> RequestQueue;

        private readonly ISubject<IErosPodRequest> RequestSubject;

        public ErosRequestQueue()
        {
            RequestQueue = new BlockingCollection<IErosPodRequest>(new ConcurrentQueue<IErosPodRequest>());
        }

        public void Startup()
        {
            // TODO: load here
        }

        public void Shutdown()
        {
            RequestQueue.CompleteAdding();
        }

        public IErosPodRequest Enqueue(IErosPodRequest request)
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