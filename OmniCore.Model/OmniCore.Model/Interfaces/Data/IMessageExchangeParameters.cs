using OmniCore.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Data
{
    public interface IMessageExchangeParameters
    {
        long? Id { get; set; }
        Guid PodId { get; set; }
        DateTime Created { get; set; }

        TxPower? TransmissionLevelOverride { get; }
        bool AllowAutoLevelAdjustment { get; }
    }
}
