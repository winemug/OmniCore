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
        Task UpdateStatus(IMessageExchangeProgress progress, StatusRequestType requestType = StatusRequestType.Standard);
        Task AcknowledgeAlerts(IMessageExchangeProgress progress, byte alertMask);
        Task ConfigureAlerts(IMessageExchangeProgress progress, AlertConfiguration[] alertConfigurations);

        Task Bolus(IMessageExchangeProgress progress, decimal bolusAmount);
        Task CancelBolus(IMessageExchangeProgress progress);
        Task SetTempBasal(IMessageExchangeProgress progress, decimal basalRate, decimal durationInHours);
        Task CancelTempBasal(IMessageExchangeProgress progress);
        Task StartExtendedBolus(IMessageExchangeProgress progress, decimal bolusAmount, decimal durationInHours);
        Task CancelExtendedBolus(IMessageExchangeProgress progress);
        Task SetBasalSchedule(IMessageExchangeProgress progress, decimal[] schedule, int utcOffsetInMinutes);
        Task SuspendBasal(IMessageExchangeProgress progress);

        Task Pair(IMessageExchangeProgress progress, int utcTimeOffsetMinutes);
        Task Activate(IMessageExchangeProgress progress);
        Task InjectAndStart(IMessageExchangeProgress progress, decimal[] basalSchedule, int utcOffsetInMinutes);

        Task Deactivate(IMessageExchangeProgress progress);
    }
}
