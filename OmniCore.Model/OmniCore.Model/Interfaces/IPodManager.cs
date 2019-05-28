using OmniCore.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IPodManager
    {
        IPod Pod { get; }
        Task UpdateStatus(IMessageProgress progress, CancellationToken ct, StatusRequestType requestType = StatusRequestType.Standard);
        Task AcknowledgeAlerts(IMessageProgress progress, CancellationToken ct, byte alertMask);
        Task Bolus(IMessageProgress progress, CancellationToken ct, decimal bolusAmount);
        Task CancelBolus(IMessageProgress progress, CancellationToken ct);
    }
}
