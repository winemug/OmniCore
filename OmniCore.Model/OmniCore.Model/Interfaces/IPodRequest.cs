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
        Guid RequestId { get; }
        RequestType PodRequestType { get; }
        Guid PodId { get; }
        string ToJson();
    }
}
