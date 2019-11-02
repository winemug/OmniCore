using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IBackgroundTaskFactory
    {
        IBackgroundTask CreateBackgroundTask(Action action);
    }
}
