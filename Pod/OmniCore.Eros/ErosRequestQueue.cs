using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services.Facade;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Eros
{
    public class ErosRequestQueue : IServerResolvable
    {
        private BlockingCollection<ErosPodRequest> RequestQueue;

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
                throw new OmniCoreWorkflowException(FailureType.Internal, "Queue is shutting down, no new jobs can be added.");
            
            RequestQueue.Add(request);
            return request;
        }

        private async Task ConsumeQueue()
        {
            while (!RequestQueue.IsCompleted)
            {
                if (RequestQueue.TryTake(out var request))
                {
                    await request.ExecuteRequest();
                }
            }
        }
    }
}
