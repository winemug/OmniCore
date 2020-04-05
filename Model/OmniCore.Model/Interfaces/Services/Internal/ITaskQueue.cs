using System.Collections.Generic;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services.Facade;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface ITaskQueue : IServerResolvable
    {
        void Startup();
        void Shutdown();
        IEnumerable<ITask> List();
        ITask Enqueue(ITask task);
    }
}