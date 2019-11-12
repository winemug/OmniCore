using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Repository.Enums;
using SQLite;

namespace OmniCore.Repository.Entities
{
    public class PodRequest : UpdateableEntity
    {
        [Indexed]
        public long PodId { get; set; }
        [Indexed]
        public RequestState RequestStatus { get; set; }
        public RequestType RequestType { get; set; }
        public string Parameters { get; set; }
        public DateTimeOffset? StartEarliest { get; set; }
        public DateTimeOffset? StartLatest { get; set; }

        public DateTimeOffset? Started { get; set; }
        public DateTimeOffset? ResultReceived { get; set; }

        public FailureType? FailureType { get; }
        public string ExceptionText { get; }
    }
}
