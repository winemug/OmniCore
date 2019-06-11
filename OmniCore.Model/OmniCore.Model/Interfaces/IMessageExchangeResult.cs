using OmniCore.Model.Enums;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IMessageExchangeResult
    {
        long? Id { get; set; }
        Guid PodId { get; set; }

        DateTime? RequestTime { get; set; }
        RequestSource Source { get; set; }
        RequestType Type { get; set; }
        string Parameters { get; set; }

        DateTime? ResultTime { get; set; }

        bool Success { get; set; }
        FailureType Failure { get; set;  }

        Exception Exception { get; set;  }

        IMessageExchangeStatistics Statistics { get; set; }
        IPodAlertStates AlertStates { get; set; }
        IPodBasalSchedule BasalSchedule { get; set; }
        IPodFault Fault { get; set; }
        IPodStatus Status { get; set; }
        IPodUserSettings UserSettings { get; set; }

    }
}
