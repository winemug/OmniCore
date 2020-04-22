// using System.Collections.Concurrent;
// using System.Reactive.Subjects;
// using System.Threading.Tasks;
// using OmniCore.Model.Enumerations;
// using OmniCore.Model.Exceptions;
// using OmniCore.Model.Interfaces.Common;
// using OmniCore.Model.Interfaces.Services;
// using OmniCore.Model.Interfaces.Services.Facade;
// using OmniCore.Model.Interfaces.Services.Internal;
//
// namespace OmniCore.Eros
// {
//     public class ErosRequestQueue 
//     {
//         private readonly BlockingCollection<IErosPodTask> RequestQueue;
//
//         private readonly ISubject<IErosPodTask> RequestSubject;
//
//         public ErosRequestQueue()
//         {
//             RequestQueue = new BlockingCollection<IErosPodTask>(new ConcurrentQueue<IErosPodTask>());
//         }
//
//         public void Startup()
//         {
//             // TODO: load here
//         }
//
//         public void Shutdown()
//         {
//             RequestQueue.CompleteAdding();
//         }
//
//         public IErosPodTask Enqueue(IErosPodTask task)
//         {
//             if (RequestQueue.IsAddingCompleted)
//                 throw new OmniCoreWorkflowException(FailureType.Internal,
//                     "Queue is shutting down, no new jobs can be added.");
//
//             RequestQueue.Add(task);
//             return task;
//         }
//
//         private async Task ConsumeQueue()
//         {
//             while (!RequestQueue.IsCompleted)
//                 if (RequestQueue.TryTake(out var request))
//                     await request.ExecuteRequest();
//         }
//     }
// }