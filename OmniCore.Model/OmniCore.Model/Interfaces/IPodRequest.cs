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
        Guid Id { get; }
        Guid PodId { get; }
        DateTimeOffset Created { get; }
        RequestType RequestType { get; }
        IPodRequestParameters Parameters { get; }
    }
}
