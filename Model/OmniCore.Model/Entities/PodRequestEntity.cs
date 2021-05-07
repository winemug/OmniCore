using System;
using System.Collections.Generic;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Entities
{
    public class PodRequestEntity : Entity
    {
        public PodEntity Pod { get; set; }
        public RequestType RequestType { get; set; }
        public string RequestParameters { get; set; }
        
        public TaskState State { get; set; }
        public TaskResult Result { get; set; }
        
        public DateTime? ScheduledStartEarliest { get; set; }
        public DateTime? ScheduledStartLatest { get; set; }

        public DateTime? Started { get; set; }
        public DateTime? Ended { get; set; }

        public FailureType? FailureType { get; set; }
        public string ErrorText { get; set; }
    }
}