using System;
using OmniCore.Model.Enums;

namespace OmniCore.Model.Interfaces
{
    public interface IMessageExchangeParameters
    {
        long? Id { get; set; }
        Guid PodId { get; set; }
        DateTimeOffset Created { get; set; }
        TxPower? TransmissionLevelOverride { get; }
        bool AllowAutoLevelAdjustment { get; }
    }
}
