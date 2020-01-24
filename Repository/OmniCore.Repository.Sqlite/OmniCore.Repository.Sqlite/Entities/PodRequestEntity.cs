using System;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;

namespace OmniCore.Repository.Sqlite.Entities
{
    public class PodRequestEntity : Entity, IPodRequestEntity
    {
        public RequestState RequestStatus { get; set; }
        public RequestType RequestType { get; set; }
        public string Parameters { get; set; }
        public DateTimeOffset? StartEarliest { get; set; }
        public DateTimeOffset? StartLatest { get; set; }
        public DateTimeOffset? Started { get; set; }
        public DateTimeOffset? ResultReceived { get; set; }
        public FailureType? FailureType { get; set; }
        public string ErrorText { get; set; }
        public IPodEntity Pod { get; set; }
    }
}