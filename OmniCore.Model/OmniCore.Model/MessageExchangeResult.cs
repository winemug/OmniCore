using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using SQLiteNetExtensions.Attributes;

namespace OmniCore.Model
{
    [Table("Result")]
    public class MessageExchangeResult : IMessageExchangeResult
    {
        [PrimaryKey]
        public long? Id { get; set; }

        public Guid PodId { get; set; }

        public DateTime? RequestTime { get; set; }
        public DateTime? ResultTime { get; set; }

        public RequestSource Source { get; set; }
        public RequestType Type { get; set; }
        public string Parameters { get; set; }

        public bool Success { get; set; }

        public FailureType Failure { get; set; }

        [Ignore]
        public Exception Exception { get; set; }

        public long? StatisticsId { get; set; }
        [OneToOne(nameof(StatisticsId))]
        public IMessageExchangeStatistics Statistics { get; set; }

        public long? AlertStatesId { get; set; }
        [OneToOne(nameof(AlertStatesId))]
        public IPodAlertStates AlertStates { get; set; }

        public long? BasalScheduleId { get; set; }
        [OneToOne(nameof(BasalScheduleId))]
        public IPodBasalSchedule BasalSchedule { get; set; }

        public long? FaultId { get; set; }
        [OneToOne(nameof(FaultId))]
        public IPodFault Fault { get; set; }

        public long? StatusId { get; set; }
        [OneToOne(nameof(StatusId))]
        public IPodStatus Status { get; set; }

        public long? UserSettingsId { get; set; }
        [OneToOne(nameof(UserSettingsId))]
        public IPodUserSettings UserSettings { get; set; }

    }
}
