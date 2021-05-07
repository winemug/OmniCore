﻿using System;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface ITaskSchedule 
    {
        ITask RelatedTask { get; set; }
        TimeSpan? ExecuteRelativeEarliest { get; set; }
        DateTimeOffset? StartExecuteLatest { get; set; }
    }
}