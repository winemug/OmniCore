using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Attributes;
using OmniCore.Model.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Operational
{
    public interface IPodRequest
    {
        IPodRequestEntity Entity { get; }
        Task<bool> WaitForResult(CancellationToken cancellationToken);
        Task<bool> Cancel();
    }
}
