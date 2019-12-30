using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ITaskSchedule
    {
        ITask RelatedTask { get; set; }
        TimeSpan? ExecuteRelativeEarliest { get; set; }
        DateTimeOffset? StartExecuteLatest { get; set; }
    }
}
