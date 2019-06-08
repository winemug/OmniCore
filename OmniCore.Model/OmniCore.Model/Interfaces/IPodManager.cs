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
        Task<IConversation> StartConversation(int timeout=0);

        Task UpdateStatus(IConversation conversation, StatusRequestType requestType = StatusRequestType.Standard);
        Task AcknowledgeAlerts(IConversation conversation, byte alertMask);
        Task ConfigureAlerts(IConversation conversation, AlertConfiguration[] alertConfigurations);

        Task Bolus(IConversation conversation, decimal bolusAmount);
        Task CancelBolus(IConversation conversation);
        Task SetTempBasal(IConversation conversation, decimal basalRate, decimal durationInHours);
        Task CancelTempBasal(IConversation conversation);
        Task StartExtendedBolus(IConversation conversation, decimal bolusAmount, decimal durationInHours);
        Task CancelExtendedBolus(IConversation conversation);
        Task SetBasalSchedule(IConversation conversation, decimal[] schedule, int utcOffsetInMinutes);
        Task SuspendBasal(IConversation conversation);

        Task Pair(IConversation conversation, int utcTimeOffsetMinutes);
        Task Activate(IConversation conversation);
        Task InjectAndStart(IConversation conversation, decimal[] basalSchedule, int utcOffsetInMinutes);

        Task Deactivate(IConversation conversation);
    }
}
