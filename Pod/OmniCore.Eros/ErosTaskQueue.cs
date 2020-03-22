using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Model.Interfaces.Services.Facade;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Eros
{
    public class ErosTaskQueue : ITaskQueue
    {
        private BlockingCollection<ITask> Tasks;

        public ErosTaskQueue()
        {
            Tasks = new BlockingCollection<ITask>(new ConcurrentQueue<ITask>());
        }

        public void Startup()
        {
            // TODO: load here
        }

        public void Shutdown()
        {

        }

        public IEnumerable<ITask> List()
        {
            return Tasks.AsEnumerable();
        }

        public ITask Enqueue(ITask task)
        {
            Tasks.Add(task);
            return task;
        }

        private async Task ConsumeQueue()
        {
            while (!Tasks.IsCompleted)
            {
                if (Tasks.TryTake(out ITask task))
                {
                    await task.Run();
                }
            }
        }
    }
}
