using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enums;

namespace OmniCore.Model.Interfaces
{
    public interface IPod : IPodVariables
    {
        Task UpdateStatus(IMessageProgress progress, CancellationToken ct, StatusRequestType requestType);
        Task AcknowledgeAlerts(IMessageProgress progress, CancellationToken ct, byte alertMask);
        Task Bolus(IMessageProgress progress, CancellationToken ct, decimal bolusAmount);
        Task CancelBolus(IMessageProgress progress, CancellationToken ct);

        Task<bool> WithLotAndTid(uint lot, uint tid);
    }
}