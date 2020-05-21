using System;
using System.Collections.Generic;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Entities
{
    public class PodRequestEntity : Entity
    {
        public PodEntity Pod { get; set; }
        public RequestType RequestType { get; set; }
        public string Parameters { get; set; }

        public RequestState RequestStatus { get; set; }
        public DateTime? StartEarliest { get; set; }
        public DateTime? StartLatest { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? ResultReceived { get; set; }
        public FailureType? FailureType { get; set; }
        public string ErrorText { get; set; }
    }
}