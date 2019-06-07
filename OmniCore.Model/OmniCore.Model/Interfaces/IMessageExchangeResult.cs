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

        bool Success { get; set; }

        FailureType FailureType { get; set;  }

        Exception Exception { get; set;  }
    }
}
