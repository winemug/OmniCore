using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enums;

namespace OmniCore.Model.Interfaces
{
    public interface IPodRequest
    {
        // Task<IPodResult> Execute(IPod pod, IRadio radio);
        // IList<IPodRequest> Enlist(IList<IPodRequest> pendingRequests);
        Guid Id { get; }
        Guid PodId { get; }
        DateTimeOffset Created { get; }
        RequestType RequestType { get; }
        string Parameters { get; }
    }
}
