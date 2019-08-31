using System;
using System.Threading.Tasks;
using OmniCore.Model.Enums;

namespace OmniCore.Model.Interfaces
{
    public interface IPod
    {
        Guid Id { get; set; }
        DateTimeOffset Created { get; set; }
        DateTimeOffset Updated { get; set; }
        bool Archived { get; set; }
        string[] ProviderSpecificRadioIds { get; set; }

        uint? Lot { get; set; }
        uint? Serial { get; set; }
        uint RadioAddress { get; set; }
        int MessageSequence { get; set; }

        string VersionPi { get; set; }
        string VersionPm { get; set; }
        string VersionUnknown { get; set; }

        bool Faulted { get; set; }
        PodFault FaultCode { get; set; }
        DateTimeOffset? FaultDate { get; set; }

        DateTimeOffset? ActivationDate { get; set; }
        DateTimeOffset? InsertionDate { get; set; }
        decimal? ReservoirUsedForPriming { get; set; }

        decimal TotalDelivered { get; set; }
        decimal Reservoir { get; set; }
        uint ActiveMinutes { get; set; }

        PodProgress Progress { get; set; }

        BasalState BasalState { get; set; }

        DateTimeOffset? TempBasalStart { get; set; }
        DateTimeOffset? TempBasalEnd { get; set; }
        decimal? TempBasalDurationInHours { get; set; }
        decimal? TempBasalRate { get; set; }

        DateTimeOffset? SuspendBasalStart { get; set; }
        DateTimeOffset? SuspendBasalEnd { get; set; }

        BolusState BolusState { get; set; }

        DateTimeOffset? BolusStart { get; set; }
        DateTimeOffset? BolusEnd { get; set; }
        decimal? BolusAmount { get; set; }
        decimal? BolusDelivered { get; set; }
        decimal? BolusRemaining { get; set; }

        DateTimeOffset? ExtendedBolusStart { get; set; }
        DateTimeOffset? ExtendedBolusEnd { get; set; }
        decimal? ExtendedBolusAmount { get; set; }
        decimal? ExtendedBolusDelivered { get; set; }
        decimal? ExtendedBolusRemaining { get; set; }

        IBasalSchedule BasalSchedule { get; set; }
        IReminderConfiguration[] Reminders { get; set; }

        Task<IPodRequestPair> CreatePairRequest(uint radioAddress);
    }
}