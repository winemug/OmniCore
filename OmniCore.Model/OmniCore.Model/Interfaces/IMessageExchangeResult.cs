using OmniCore.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IMessageExchangeResult
    {
        bool Success { get; }
        FailureType FailureType { get; }
        Exception Exception { get; }
        IMessageExchangeStatistics Statistics { get; }
    }
}
