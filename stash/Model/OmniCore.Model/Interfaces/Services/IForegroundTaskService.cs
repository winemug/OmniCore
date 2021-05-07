using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IForegroundTaskService
    {
        Task ExecuteTask(IForegroundTask foregroundTask, CancellationToken cancellationToken);
    }
}
