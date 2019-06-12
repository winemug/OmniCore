using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using SQLiteNetExtensions.Attributes;
using OmniCore.Model.Interfaces.Data;

namespace OmniCore.Model.Eros.Data
{
    public class ErosMessageExchangeResult : IMessageExchangeResult
    {
        [PrimaryKey, AutoIncrement]
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

        public long? ParametersId { get; set; }
        [OneToOne(nameof(ParametersId))]
        public IMessageExchangeParameters ExchangeParameters { get; set; }

        public long? AlertStatesId { get; set; }
        [OneToOne(nameof(AlertStatesId))]
        public IAlertStates AlertStates { get; set; }

        public long? BasalScheduleId { get; set; }
        [OneToOne(nameof(BasalScheduleId))]
        public IBasalSchedule BasalSchedule { get; set; }

        public long? FaultId { get; set; }
        [OneToOne(nameof(FaultId))]
        public IFault Fault { get; set; }

        public long? StatusId { get; set; }
        [OneToOne(nameof(StatusId))]
        public IStatus Status { get; set; }

        public long? UserSettingsId { get; set; }
        [OneToOne(nameof(UserSettingsId))]
        public IUserSettings UserSettings { get; set; }

    }
}
