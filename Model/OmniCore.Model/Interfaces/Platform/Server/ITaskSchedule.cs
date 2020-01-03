using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface ITaskSchedule : IServerResolvable
    {
        ITask RelatedTask { get; set; }
        TimeSpan? ExecuteRelativeEarliest { get; set; }
        DateTimeOffset? StartExecuteLatest { get; set; }
    }
}
