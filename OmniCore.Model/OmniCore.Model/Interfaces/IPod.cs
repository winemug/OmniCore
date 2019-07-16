using System;
using System.Threading.Tasks;
using OmniCore.Model.Enums;

namespace OmniCore.Model.Interfaces
{
    public interface IPod
    {
        Guid Id { get; set; }
        DateTimeOffset Created { get; set; }

        uint? Lot { get; set; }
        uint? Serial { get; set; }
        uint RadioAddress { get; set; }
        int MessageSequence { get; set; }

        DateTimeOffset? ActivationDate { get; set; }
        DateTimeOffset? InsertionDate { get; set; }
        string VersionPi { get; set; }
        string VersionPm { get; set; }
        string VersionUnknown { get; set; }
        decimal? ReservoirUsedForPriming { get; set; }

        bool Archived { get; set; }

        IMessageExchangeResult LastTempBasalResult { get; set; }
        IAlertStates LastAlertStates { get; set; }
        IBasalSchedule LastBasalSchedule { get; set; }
        IFault LastFault { get; set; }
        IStatus LastStatus { get; set; }
        IUserSettings LastUserSettings { get; set; }

        Task<IConversation> StartConversation(IMessageExchangeProvider messageExchangeProvider,
            string intent,
            int timeout = 0,
            RequestSource source = RequestSource.OmniCoreUser);

        Task UpdateStatus(IConversation conversation, StatusRequestType requestType = StatusRequestType.Standard, int? timeout = null);
        Task AcknowledgeAlerts(IConversation conversation, byte alertMask);
        Task ConfigureAlerts(IConversation conversation, AlertConfiguration[] alertConfigurations);
        Task Bolus(IConversation conversation, decimal bolusAmount, bool waitForBolusToFinish = true);
        Task CancelBolus(IConversation conversation);
        Task SetTempBasal(IConversation conversation, decimal basalRate, decimal durationInHours);
        Task CancelTempBasal(IConversation conversation);
        Task StartExtendedBolus(IConversation conversation, decimal bolusAmount, decimal durationInHours);
        Task CancelExtendedBolus(IConversation conversation);
        Task SetBasalSchedule(IConversation conversation, IProfile profile);
        Task SuspendBasal(IConversation conversation);
        Task Pair(IConversation conversation, IProfile profile);
        Task Activate(IConversation conversation);
        Task InjectAndStart(IConversation conversation, IProfile profile);
        Task Deactivate(IConversation conversation);
    }
}