using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface ITaskQueue : IServerResolvable
    {
        void Startup();
        void Shutdown();
        IEnumerable<ITask> List();
        void Enqueue(ITask task);
    }
}
