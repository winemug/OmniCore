using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ITaskQueue
    {
        Task Startup();
        Task Shutdown();
        Task<IList<ITask>> List();
        Task Enqueue(ITask task);
    }
}
