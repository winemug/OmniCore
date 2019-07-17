using System;
using OmniCore.Model.Enums;

namespace OmniCore.Model.Interfaces
{
    public interface IMessageExchangeResult
    {
        long? Id { get; set; }
        Guid PodId { get; set; }

        DateTimeOffset? RequestTime { get; set; }
        RequestSource Source { get; set; }
        RequestType Type { get; set; }
        string Parameters { get; set; }

        DateTimeOffset? ResultTime { get; set; }

        bool Success { get; set; }
        FailureType Failure { get; set;  }
        Exception Exception { get; set;  }

        IAlertStates AlertStates { get; set; }
        IBasalSchedule BasalSchedule { get; set; }
        IFault Fault { get; set; }
        IStatus Status { get; set; }
        IUserSettings UserSettings { get; set; }

        long? StatisticsId { get; set; }
        long? ParametersId { get; set; }
        long? AlertStatesId { get; set; }
        long? BasalScheduleId { get; set; }
        long? FaultId { get; set; }
        long? StatusId { get; set; }
        long? UserSettingsId { get; set; }

    }
}
