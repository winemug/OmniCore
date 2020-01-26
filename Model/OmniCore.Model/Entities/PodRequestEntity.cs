using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Entities
{
    public class PodRequestEntity : Entity
    {
        public PodEntity Pod { get; set; }
        public RequestState RequestStatus { get; set; }
        public RequestType RequestType { get; set; }
        public string Parameters { get; set; }
        public DateTimeOffset? StartEarliest { get; set; }
        public DateTimeOffset? StartLatest { get; set; }
        public DateTimeOffset? Started { get; set; }
        public DateTimeOffset? ResultReceived { get; set; }
        public FailureType? FailureType { get; set; }
        public string ErrorText { get; set; }
    }
}
