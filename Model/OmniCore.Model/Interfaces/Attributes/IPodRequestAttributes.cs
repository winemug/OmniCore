using OmniCore.Model.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Attributes
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
