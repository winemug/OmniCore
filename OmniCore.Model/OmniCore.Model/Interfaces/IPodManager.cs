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
        Task<IMessageExchangeResult> UpdateStatus(IMessageExchangeProgress progress, CancellationToken ct, StatusRequestType requestType = StatusRequestType.Standard);
        Task<IMessageExchangeResult> AcknowledgeAlerts(IMessageExchangeProgress progress, CancellationToken ct, byte alertMask);
        Task<IMessageExchangeResult> Bolus(IMessageExchangeProgress progress, CancellationToken ct, decimal bolusAmount);
        Task<IMessageExchangeResult> CancelBolus(IMessageExchangeProgress progress, CancellationToken ct);
        Task<IMessageExchangeResult> Deactivate(IMessageExchangeProgress progress, CancellationToken ct);
    }
}
