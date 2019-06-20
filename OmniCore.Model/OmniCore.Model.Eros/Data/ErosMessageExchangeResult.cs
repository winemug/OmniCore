using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Utilities;
using OmniCore.Mobile.Base;

namespace OmniCore.Model.Eros.Data
{
    public class ErosMessageExchangeResult : PropertyChangedImpl, IMessageExchangeResult
    {
        private DateTimeOffset? requestTime;
        private DateTimeOffset? resultTime;
        private RequestSource source;
        private RequestType type;
        private bool success;
        private FailureType failure;
        private IMessageExchangeStatistics statistics;
        private IStatus status;

        [PrimaryKey, AutoIncrement]
        public long? Id { get; set; }

        public Guid PodId { get; set; }

        public DateTimeOffset? RequestTime { get => requestTime; set => SetProperty(ref requestTime, value); }
        [Indexed]
        public DateTimeOffset? ResultTime { get => resultTime; set => SetProperty(ref resultTime,  value); }

        public RequestSource Source { get => source; set => SetProperty(ref source, value); }
        public RequestType Type { get => type; set => SetProperty(ref type, value); }
        public string Parameters { get; set; }

        public bool Success { get => success; set => SetProperty(ref success, value); }

        public FailureType Failure { get => failure; set => SetProperty(ref failure, value); }

        [Ignore]
        public Exception Exception { get; set; }

        public long? StatisticsId { get; set; }
        [Ignore]
        public IMessageExchangeStatistics Statistics { get => statistics; set => SetProperty(ref statistics, value); }

        public long? ParametersId { get; set; }
        [Ignore]
        public IMessageExchangeParameters ExchangeParameters { get; set; }

        public long? AlertStatesId { get; set; }
        [Ignore]
        public IAlertStates AlertStates { get; set; }

        public long? BasalScheduleId { get; set; }
        [Ignore]
        public IBasalSchedule BasalSchedule { get; set; }

        public long? FaultId { get; set; }
        [Ignore]
        public IFault Fault { get; set; }

        public long? StatusId { get; set; }
        [Ignore]
        public IStatus Status { get => status; set => SetProperty(ref status, value); }

        public long? UserSettingsId { get; set; }
        [Ignore]
        public IUserSettings UserSettings { get; set; }
    }
}
