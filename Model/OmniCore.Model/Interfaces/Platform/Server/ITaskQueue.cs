using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface ITaskQueue : IServerResolvable
    {
        void Startup();
        void Shutdown();
        IEnumerable<ITask> List();
        ITask Enqueue(ITask task);
    }
}
