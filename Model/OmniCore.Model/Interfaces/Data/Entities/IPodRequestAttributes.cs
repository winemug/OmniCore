using System;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Platform.Common.Data.Entities
{
    public interface IPodRequestAttributes
    {
        RequestState RequestStatus { get; set; }
        RequestType RequestType { get; set; }
        string Parameters { get; set; }
        DateTimeOffset? StartEarliest { get; set; }
        DateTimeOffset? StartLatest { get; set; }

        DateTimeOffset? Started { get; set; }
        DateTimeOffset? ResultReceived { get; set; }

        FailureType? FailureType { get; set; }
        string ErrorText { get; set; }

    }
}
