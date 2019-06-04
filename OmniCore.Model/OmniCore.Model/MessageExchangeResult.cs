using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model
{
    public class MessageExchangeResult : IMessageExchangeResult
    {
        public int? Id { get; set; }

        public Guid PodId { get; set; }

        public DateTime? ResultTime { get; set; }

        public bool Success { get; private set; }

        public Exception Exception { get; private set; }

        public FailureType FailureType { get; private set; }

        public IMessageExchangeStatistics Statistics { get; set; }

        public MessageExchangeResult(Exception exception)
        {
            Success = false;
            var oe = exception as OmniCoreException;
            FailureType = oe?.FailureType ?? FailureType.Unknown;
            Exception = exception;
        }

        public MessageExchangeResult()
        {
            Success = true;
            FailureType = FailureType.None;
            Exception = null;
        }
    }
}
