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
        Task<IMessageExchangeResult> Bolus(IMessageExchangeProgress progress, decimal bolusAmount);
        //Task<IMessageExchangeResult> CancelBolus(IMessageExchangeProgress progress);
        Task<IMessageExchangeResult> Deactivate(IMessageExchangeProgress progress);
        Task<IMessageExchangeResult> Pair(IMessageExchangeProgress progress, int utcTimeOffsetMinutes);
        Task<IMessageExchangeResult> Activate(IMessageExchangeProgress progress);
        Task<IMessageExchangeResult> InjectAndStart(IMessageExchangeProgress progress, decimal[] basalSchedule, int utcOffsetInMinutes);
    }
}
