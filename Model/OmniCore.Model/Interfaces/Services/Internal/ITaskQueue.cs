using System.Collections.Generic;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface ITaskQueue 
    {
        void Startup();
        void Shutdown();
        IEnumerable<ITask> List();
        ITask Enqueue(ITask task);
    }
}