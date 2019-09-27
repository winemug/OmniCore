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
        RequestType RequestType { get; }
        IPodRequestParameters Parameters { get; }
        DateTimeOffset? StartEarliest { get; set; }
        DateTimeOffset? StartLatest { get; set; }
    }
}
