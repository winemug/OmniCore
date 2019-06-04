using OmniCore.Model.Enums;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IMessageExchangeResult
    {
        int? Id { get; set; }
        Guid PodId { get; set; }
        DateTime? ResultTime { get; set; }

        bool Success { get; }
        FailureType FailureType { get; }

        [Ignore]
        Exception Exception { get; }

        [Ignore]
        IMessageExchangeStatistics Statistics { get; }
    }
}
