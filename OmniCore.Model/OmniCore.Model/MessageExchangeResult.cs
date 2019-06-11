using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

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
        [Ignore]
        public IMessageExchangeStatistics Statistics { get; set; }
        [Ignore]
        public IPodAlertStates AlertStates { get; set; }
        [Ignore]
        public IPodBasalSchedule BasalSchedule { get; set; }
        [Ignore]
        public IPodFault Fault { get; set; }
        [Ignore]
        public IPodStatus Status { get; set; }
        [Ignore]
        public IPodUserSettings UserSettings { get; set; }

    }
}
