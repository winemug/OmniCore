using System;
using OmniCore.Model.Interfaces.Base;
using OmniCore.Model.Interfaces.Services.Facade;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface ITaskSchedule : IServerResolvable
    {
        ITask RelatedTask { get; set; }
        TimeSpan? ExecuteRelativeEarliest { get; set; }
        DateTimeOffset? StartExecuteLatest { get; set; }
    }
}
