using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enums;

namespace OmniCore.Model.Interfaces
{
    public interface IPodRequest<T> : IEntity where T : IPod, new()
    {
        T Pod { get; }
        RequestState RequestStatus { get; }
        RequestType RequestType { get; set; }
        IPodRequestParameters Parameters { get; set; }
        DateTimeOffset? StartEarliest { get; set; }
        DateTimeOffset? StartLatest { get; set; }

        DateTimeOffset? Started { get; set; }
        DateTimeOffset? ResultReceived { get; set; }

        FailureType? FailureType { get; }
        string ExceptionText { get; }
    }
}
