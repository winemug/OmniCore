using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface ITaskSchedule
    {
        ITask RelatedTask { get; set; }
        TimeSpan? ExecuteRelativeEarliest { get; set; }
        DateTimeOffset? StartExecuteLatest { get; set; }
    }
}
