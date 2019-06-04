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
        Task<IMessageExchangeResult> UpdateStatus(IMessageExchangeProgress progress, StatusRequestType requestType = StatusRequestType.Standard);
        Task<IMessageExchangeResult> AcknowledgeAlerts(IMessageExchangeProgress progress, byte alertMask);
        Task<IMessageExchangeResult> ConfigureAlerts(IMessageExchangeProgress progress, AlertConfiguration[] alertConfigurations);

        Task<IMessageExchangeResult> Bolus(IMessageExchangeProgress progress, decimal bolusAmount);
        Task<IMessageExchangeResult> CancelBolus(IMessageExchangeProgress progress);
        Task<IMessageExchangeResult> SetTempBasal(IMessageExchangeProgress progress, decimal basalRate, decimal durationInHours);
        Task<IMessageExchangeResult> CancelTempBasal(IMessageExchangeProgress progress);
        Task<IMessageExchangeResult> StartExtendedBolus(IMessageExchangeProgress progress, decimal bolusAmount, decimal durationInHours);
        Task<IMessageExchangeResult> CancelExtendedBolus(IMessageExchangeProgress progress);
        Task<IMessageExchangeResult> SetBasalSchedule(IMessageExchangeProgress progress, decimal[] schedule, int utcOffsetInMinutes);
        Task<IMessageExchangeResult> SuspendBasal(IMessageExchangeProgress progress);

        Task<IMessageExchangeResult> Pair(IMessageExchangeProgress progress, int utcTimeOffsetMinutes);
        Task<IMessageExchangeResult> Activate(IMessageExchangeProgress progress);
        Task<IMessageExchangeResult> InjectAndStart(IMessageExchangeProgress progress, decimal[] basalSchedule, int utcOffsetInMinutes);

        Task<IMessageExchangeResult> Deactivate(IMessageExchangeProgress progress);
    }
}
